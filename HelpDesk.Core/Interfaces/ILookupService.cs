using HelpDesk.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ILookupService
    {
        Task<ApiResponse<List<object>>> GetStatusesAsync();
        Task<ApiResponse<List<object>>> GetPrioritiesAsync();

        Task<ApiResponse<List<object>>> GetRolesAsync();


    }
}
