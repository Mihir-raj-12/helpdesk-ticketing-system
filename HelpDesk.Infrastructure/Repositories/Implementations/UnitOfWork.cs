using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;

namespace HelpDesk.Infrastructure.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public ITicketRepository Tickets { get; private set; }
        public ICommentRepository Comments { get; private set; }
        public IAuditRepository AuditLogs { get; private set; }
        public IGenericRepository<Category> Categories { get; private set; }
        public IGenericRepository<AuditDetail> AuditDetails { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Tickets = new TicketRepository(_context);
            Comments = new CommentRepository(_context);
            AuditLogs = new AuditRepository(_context);
            Categories = new GenericRepository<Category>(_context);
            AuditDetails = new GenericRepository<AuditDetail>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
