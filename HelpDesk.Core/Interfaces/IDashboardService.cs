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
    }
}
