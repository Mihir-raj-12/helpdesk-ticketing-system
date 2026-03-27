using HelpDesk.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetTicketsByUserIdAsync(string userId);
        Task<IEnumerable<Ticket>> GetTicketsByAgentIdAsync(string agentId);
        Task<Ticket?> GetTicketWithDetailsAsync(int ticketId);
        Task<IEnumerable<Ticket>> GetAllTicketsWithDetailsAsync();
    }
}
