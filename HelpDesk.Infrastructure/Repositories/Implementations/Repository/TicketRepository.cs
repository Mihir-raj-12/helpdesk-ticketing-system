using HelpDesk.Core.Entities;
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
    }
}
