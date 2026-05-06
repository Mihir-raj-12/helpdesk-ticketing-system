using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.BackgroundJobs
{
    public class RecurringTicketWorker : BackgroundService
    {
        private readonly ILogger<RecurringTicketWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public RecurringTicketWorker(ILogger<RecurringTicketWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recurring Ticket Engine started.");

            // We check every 1 minute. 
            // In a real enterprise app, you might only check hourly to save database queries, 
            // but 1 minute is perfect for us to test it!
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessRecurringTicketsAsync();
            }
        }

        private async Task ProcessRecurringTicketsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Find blueprints that are due (or overdue) to run
            var dueTemplates = await context.RecurringTickets
                .Where(rt => rt.NextRunDate <= DateTime.UtcNow)
                .ToListAsync();

            

            if (!dueTemplates.Any()) return;

            var newTickets = new List<Ticket>();

            foreach (var template in dueTemplates)
            {
                _logger.LogInformation($"Generating recurring ticket from template: {template.TicketTitle}");

                var user = await context.Users.FindAsync(template.RaiseOnBehalfOfUserId);
                // 2. Create the actual Ticket based on the blueprint
                var ticket = new Ticket
                {
                    Title = template.TicketTitle, // Updated property name
                    Description = template.Description,
                    CategoryId = template.CategoryId,
                    RaisedByUserId = template.RaiseOnBehalfOfUserId, // Updated property name

                    // --- THE NEW PRD ENFORCEMENTS ---
                    DepartmentId = user?.DepartmentId ?? 1, // Fallback to 'General' dept if user is weirdly null
                    Priority = template.Priority, // Dynamically pulls the priority from the template!
                    AssignedToUserId = template.AssignToUserId, // Applies pre-assignment if the template has one

                    // Logic edge case: If the template pre-assigns an agent, it skips 'Open' and goes straight to 'InProgress'
                    Status = string.IsNullOrEmpty(template.AssignToUserId) ? TicketStatus.Open : TicketStatus.InProgress,

                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow,

                    // Ideally use your SlaCalculatorService here, but if not injected yet, leave your placeholder
                    SlaDeadline = DateTime.UtcNow.AddDays(2),
                    IsActive = true
                };

                newTickets.Add(ticket);

                // 3. Reschedule the template for the future
                template.NextRunDate = CalculateNextRunDate(template.NextRunDate.Value, template.RecurrencePattern);
            }

            // 4. Add the new tickets to the database
            context.Tickets.AddRange(newTickets);

            // 5. Save everything! (This inserts new tickets AND updates the NextRunDate on the templates)
            await context.SaveChangesAsync();

            _logger.LogInformation($"Successfully generated {newTickets.Count} recurring tickets.");
        }

        // --- The Time Calculator ---
        private DateTime CalculateNextRunDate(DateTime currentRunDate, string frequency)
        {
            // We use the current run date as the baseline so it doesn't "drift" if the server is offline for a day
            return frequency.ToLower() switch
            {
                "daily" => currentRunDate.AddDays(1),
                "weekly" => currentRunDate.AddDays(7),
                "monthly" => currentRunDate.AddMonths(1),
                "yearly" => currentRunDate.AddYears(1),
                _ => currentRunDate.AddDays(1) // Default fallback
            };
        }
    }
}
