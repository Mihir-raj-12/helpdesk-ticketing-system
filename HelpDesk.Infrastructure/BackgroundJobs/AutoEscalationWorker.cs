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
            // 1. Create a fresh scope just for this run
            using var scope = _serviceProvider.CreateScope();

            // 2. Safely grab the database context
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 3. Find any tickets that missed their SLA deadline and aren't escalated yet
            var breachedTickets = await context.Tickets
                .Where(t => t.EscalationFlag == false &&
                            t.Status != TicketStatus.Resolved &&
                            t.Status != TicketStatus.Closed &&
                            t.SlaDeadline <= DateTime.UtcNow)
                .ToListAsync();

            if (!breachedTickets.Any()) return; // Nothing to do!

            // 4. Escalate them!
            foreach (var ticket in breachedTickets)
            {
                _logger.LogWarning($"Auto-Escalating Ticket ID: {ticket.Id}");

                ticket.EscalationFlag = true;
                ticket.EscalationReason = "System Auto-Escalation"; // PRD 10.4 Requirement
                ticket.EscalatedAt = DateTime.UtcNow; // <-- The missing piece!
                ticket.LastUpdatedDate = DateTime.UtcNow;

                // PRD 10.3: Automatically raise the priority to the next level
                ticket.Priority = ticket.Priority switch
                {
                    TicketPriority.Low => TicketPriority.Medium,
                    TicketPriority.Medium => TicketPriority.High,
                    TicketPriority.High => TicketPriority.Critical,
                    _ => ticket.Priority // If it's already Critical, leave it alone
                };
            }

            // 5. Save all the updates. 
            // Because of our awesome AuditInterceptor, this will automatically write to the Audit tables!
            await context.SaveChangesAsync();

            _logger.LogInformation($"Successfully auto-escalated {breachedTickets.Count} tickets.");
        }
    }
}
