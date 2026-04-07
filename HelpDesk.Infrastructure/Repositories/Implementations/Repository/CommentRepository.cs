using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Repository
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        public CommentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Comment>> GetCommentsByTicketIdAsync(int ticketId)
        {
            return await _dbSet
                .Include(c => c.User)
                .Where(c => c.TicketId == ticketId && c.IsActive)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();
        }
    }
}
