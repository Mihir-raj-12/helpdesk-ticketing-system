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
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context; 

        public IGenericRepository<Ticket> Tickets { get; private set; }

        public IGenericRepository<Category> Categories { get; private set; }

        public IGenericRepository<Comment> Comments { get; private set; }
        
        public IGenericRepository<AuditLog> AuditLogs { get; private set; }

        public IGenericRepository<AuditDetail> AuditDetails { get; private set; }   

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Tickets = new GenericRepository<Ticket>(_context);
            Categories = new GenericRepository<Category>(_context);
            Comments = new GenericRepository<Comment>(_context);
            AuditLogs = new GenericRepository<AuditLog>(_context);
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
