using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardResponseDto>> GetDashboardStatusAsync();

        // We return a tuple containing the raw file bytes, the content type, and the file name
        Task<(byte[] FileContents, string ContentType, string FileName)> ExportActionableTicketsCsvAsync();
    }
}
