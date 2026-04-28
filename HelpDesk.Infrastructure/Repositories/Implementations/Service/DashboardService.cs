using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Dashboard;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly ITicketService _ticketservice;
        private readonly ICurrentUserProvider _currentUserProvider;

        public DashboardService(ITicketService ticketservice , ICurrentUserProvider currentUserProvider)
        {
            _ticketservice = ticketservice;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ApiResponse<DashboardResponseDto>> GetDashboardStatusAsync()
        {

            var ticketsResponse = await _ticketservice.GetTicketsAsync();

            if (!ticketsResponse.IsSuccess || ticketsResponse.Data == null)
            {
                return ApiResponse<DashboardResponseDto>.Failure("Failed to load dashboard data.");
            }

            var tickets = ticketsResponse.Data;
            var now = DateTime.UtcNow;

            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);

            var stats = new DashboardResponseDto
            {
                TotalTickets = tickets.Count,

                // Look how much cleaner this is without .ToString()!
                OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
                InProgressTickets = tickets.Count(t => t.Status == TicketStatus.InProgress),
                ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
                OnHoldTickets = tickets.Count(t => t.Status == TicketStatus.OnHold),
                ClosedTickets = tickets.Count(t => t.Status == TicketStatus.Closed),

                LowPriorityTickets = tickets.Count(t => t.Priority == TicketPriority.Low),
                MediumPriorityTickets = tickets.Count(t => t.Priority == TicketPriority.Medium),
                HighPriorityTickets = tickets.Count(t => t.Priority == TicketPriority.High),
                CriticalPriorityTickets = tickets.Count(t => t.Priority == TicketPriority.Critical),

                TicketsThisMonth = tickets.Count(t => t.CreatedDate >= startOfThisMonth),
                TicketsLastMonth = tickets.Count(t => t.CreatedDate >= startOfLastMonth && t.CreatedDate < startOfThisMonth),

                TopAgents = tickets
                     // Removed .ToString() here as well!
                     .Where(t => t.Status == TicketStatus.Resolved && !string.IsNullOrEmpty(t.AssignedToUserName))
                     .GroupBy(t => t.AssignedToUserName)
                     .Select(g => new TopAgentDto
                     {
                         AgentName = g.Key,
                         ResolvedTicketsCount = g.Count()
                     })
                     .OrderByDescending(a => a.ResolvedTicketsCount)
                     .Take(5)
                     .ToList()
            };
            return ApiResponse<DashboardResponseDto>.Success(stats);
        


    }
    }
}
