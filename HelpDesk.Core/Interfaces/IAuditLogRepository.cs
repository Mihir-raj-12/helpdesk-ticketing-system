using HelpDesk.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        // Notice there is no LogAsync here anymore! 
        // We only need to READ the data, the Interceptor writes it.
        Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 100);

        Task<IEnumerable<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
