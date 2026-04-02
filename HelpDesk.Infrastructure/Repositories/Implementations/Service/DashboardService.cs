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

        public DashboardService(ITicketService ticketservice)
        {
            _ticketservice = ticketservice;
        }

        public async Task<ApiResponse<DashboardResponseDto>> GetDashboardStatusAsync(string currentUserId , string currentUserRole)
        {
            var ticketsResponse = await _ticketservice.GetTicketsAsync(currentUserId, currentUserRole);

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

                OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open.ToString()),
                InProgressTickets = tickets.Count(t => t.Status == TicketStatus.InProgress.ToString()),
                ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved.ToString()),
                OnHoldTickets = tickets.Count(t => t.Status == "OnHold"),
                ClosedTickets = tickets.Count(t => t.Status == "Closed"),

                LowPriorityTickets = tickets.Count(t => t.Priority == TicketPriority.Low.ToString()),
                MediumPriorityTickets = tickets.Count(t => t.Priority == TicketPriority.Medium.ToString()),
                HighPriorityTickets = tickets.Count(t => t.Priority == TicketPriority.High.ToString()),
                CriticalPriorityTickets = tickets.Count(t => t.Priority == "Critical"),

                TicketsThisMonth = tickets.Count(t => t.CreatedDate >= startOfThisMonth),
                TicketsLastMonth = tickets.Count(t => t.CreatedDate >= startOfLastMonth && t.CreatedDate < startOfThisMonth),

                TopAgents = tickets
                    .Where(t => t.Status == TicketStatus.Resolved.ToString() && !string.IsNullOrEmpty(t.AssignedToUserName))
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
