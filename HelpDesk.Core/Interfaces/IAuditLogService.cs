using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IAuditLogService
    {
        Task<ApiResponse<IEnumerable<AuditLogResponseDto>>> GetRecentLogsAsync();

        Task<byte[]> ExportLogsToCsvAsync(DateTime startDate, DateTime endDate);
    }
}
