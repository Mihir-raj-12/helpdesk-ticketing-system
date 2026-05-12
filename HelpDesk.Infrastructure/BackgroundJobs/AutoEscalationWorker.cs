using HelpDesk.Core.Enums;
using HelpDesk.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.BackgroundJobs
{
    public class AutoEscalationWorker : BackgroundService
    {
        private readonly ILogger<AutoEscalationWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        // We inject the factory (IServiceProvider), NOT the DbContext directly!
        public AutoEscalationWorker(ILogger<AutoEscalationWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Auto-Escalation Engine started.");

            // A PeriodicTimer is modern C# for running a loop on a strict schedule
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            // This loop runs endlessly until the application shuts down
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessSlaBreachesAsync();
            }
        }

        private async Task ProcessSlaBreachesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;
            var thirtyMinsAgo = now.AddMinutes(-30);
            var threeDaysAgo = now.AddDays(-3);

            // --- PHASE 3: EDGE CASE QUERIES (Gaps L09 & L10) ---
            var ticketsToEscalate = await context.Tickets
                .Where(t => t.EscalationFlag == false &&
                            t.Status != TicketStatus.Resolved &&
                            t.Status != TicketStatus.Closed &&
                            (
                                // Condition 1: Normal SLA Breach
                                t.SlaDeadline <= now ||

                                // Condition 2: Critical ticket unassigned for 30 minutes
                                (t.Priority == TicketPriority.Critical && t.AssignedToUserId == null && t.CreatedDate <= thirtyMinsAgo) ||

                                // Condition 3: Ticket left On Hold for more than 3 days
                                (t.Status == TicketStatus.OnHold && t.LastUpdatedDate <= threeDaysAgo)
                            ))
                .ToListAsync();

            if (!ticketsToEscalate.Any()) return;

            foreach (var ticket in ticketsToEscalate)
            {
                _logger.LogWarning($"Auto-Escalating Ticket ID: {ticket.Id}");

                // Dynamically assign the reason so the Audit Log knows exactly WHY it escalated
                if (ticket.Status == TicketStatus.OnHold && ticket.LastUpdatedDate <= threeDaysAgo)
                    ticket.EscalationReason = "System Auto-Escalation: On Hold > 3 days";
                else if (ticket.Priority == TicketPriority.Critical && ticket.AssignedToUserId == null)
                    ticket.EscalationReason = "System Auto-Escalation: Critical & Unassigned > 30 mins";
                else
                    ticket.EscalationReason = "System Auto-Escalation: SLA Breached";

                ticket.EscalationFlag = true;
                ticket.EscalatedAt = DateTime.UtcNow;

                ticket.Priority = ticket.Priority switch
                {
                    TicketPriority.Low => TicketPriority.Medium,
                    TicketPriority.Medium => TicketPriority.High,
                    TicketPriority.High => TicketPriority.Critical,
                    _ => ticket.Priority
                };
            }

            await context.SaveChangesAsync();
            _logger.LogInformation($"Successfully auto-escalated {ticketsToEscalate.Count} tickets.");
        }
    }
}
