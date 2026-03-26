using HelpDesk.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelpDesk.Core.Interfaces; 
using HelpDesk.Core.Entities;

namespace HelpDesk.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Ticket> Tickets { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Comment> Comments { get; }
        IGenericRepository<AuditLog> AuditLogs { get; }
        IGenericRepository<AuditDetail> AuditDetails { get; }

        Task<int> SaveChangesAsync(); 

    }
}
