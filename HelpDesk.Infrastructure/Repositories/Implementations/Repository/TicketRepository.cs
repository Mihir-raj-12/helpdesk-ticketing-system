using HelpDesk.Core.DTOs.Dashboard;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Repository
{
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        public TicketRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(t => t.Category)
                .Include(t => t.RaisedByUser)
                .Include(t => t.AssignedToUser)
                .Where(t => t.RaisedByUserId == userId && t.IsActive)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByAgentIdAsync(string agentId)
        {
            return await _dbSet
                .Include(t => t.Category)
                .Include(t => t.RaisedByUser)
                .Include(t => t.AssignedToUser)
                .Where(t => t.AssignedToUserId == agentId && t.IsActive)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

        public async Task<Ticket?> GetTicketWithDetailsAsync(int ticketId)
        {
            return await _dbSet
                .Include(t => t.Category)
                .Include(t => t.RaisedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.IsActive);
        }


        public async Task<IEnumerable<Ticket>> GetAllTicketsWithDetailsAsync()
        {
            return await _dbSet
                .Include(t => t.Category)
                .Include(t => t.RaisedByUser)
                .Include(t => t.AssignedToUser)
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

        public async Task<DashboardResponseDto> GetDashboardMetricsAsync(DateTime startOfThisMonth, DateTime startOfLastMonth, string userId, string userRole)
        {
            var now = DateTime.UtcNow;

            var query = _dbSet.Where(t => t.IsActive);

            if (userRole == "SupportAgent")
            {
                query = query.Where(t => t.AssignedToUserId == userId);
            }
            else if (userRole == "RegularUser")
            {
                query = query.Where(t => t.RaisedByUserId == userId);
            }

            return new DashboardResponseDto
            {
                TotalTickets = await query.CountAsync(),

                OpenTickets = await query.CountAsync(t => t.Status == TicketStatus.Open),
                InProgressTickets = await query.CountAsync(t => t.Status == TicketStatus.InProgress),
                ResolvedTickets = await query.CountAsync(t => t.Status == TicketStatus.Resolved),
                OnHoldTickets = await query.CountAsync(t => t.Status == TicketStatus.OnHold),
                ClosedTickets = await query.CountAsync(t => t.Status == TicketStatus.Closed),

                LowPriorityTickets = await query.CountAsync(t => t.Priority == TicketPriority.Low),
                MediumPriorityTickets = await query.CountAsync(t => t.Priority == TicketPriority.Medium),
                HighPriorityTickets = await query.CountAsync(t => t.Priority == TicketPriority.High),
                CriticalPriorityTickets = await query.CountAsync(t => t.Priority == TicketPriority.Critical),

                EscalatedTickets = await query.CountAsync(t => t.EscalationFlag == true),

                SlaBreachedTickets = await query.CountAsync(t =>
                    t.SlaDeadline <= now &&
                    t.Status != TicketStatus.Resolved &&
                    t.Status != TicketStatus.Closed),

                TicketsThisMonth = await query.CountAsync(t => t.CreatedDate >= startOfThisMonth),
                TicketsLastMonth = await query.CountAsync(t => t.CreatedDate >= startOfLastMonth && t.CreatedDate < startOfThisMonth),

                TopAgents = await query
                    .Where(t => t.Status == TicketStatus.Resolved && t.AssignedToUser != null)
                    .GroupBy(t => t.AssignedToUser.FullName)
                    .Select(g => new TopAgentDto
                    {
                        AgentName = g.Key ?? "Unknown Agent",
                        ResolvedTicketsCount = g.Count()
                    })
                    .OrderByDescending(a => a.ResolvedTicketsCount)
                    .Take(5)
                    .ToListAsync()
            };
        }


        public async Task<IEnumerable<Ticket>> GetActionableTicketsForExportAsync(string userId, string userRole)
        {
            var now = DateTime.UtcNow;

            // We Include the User tables so we have names for the CSV report
            var query = _dbSet
                .Include(t => t.RaisedByUser)
                .Include(t => t.AssignedToUser)
                .Where(t => t.IsActive);

            // Security check: Only export what the user is allowed to see
            if (userRole == "SupportAgent") query = query.Where(t => t.AssignedToUserId == userId);
            else if (userRole == "RegularUser") query = query.Where(t => t.RaisedByUserId == userId);

            // Filter for ONLY Escalated OR Breached tickets
            return await query
                .Where(t => t.EscalationFlag == true ||
                           (t.SlaDeadline <= now && t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed))
                .OrderByDescending(t => t.Priority) // Sort by highest priority first
                .ThenBy(t => t.CreatedDate)
                .ToListAsync();
        }
    }
}
