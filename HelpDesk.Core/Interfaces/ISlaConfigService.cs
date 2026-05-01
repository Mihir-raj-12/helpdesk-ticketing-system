using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ISlaConfigService
    {
        Task<ApiResponse<IEnumerable<SlaConfigDto>>> GetAllConfigsAsync();
        Task<ApiResponse<SlaConfigDto>> UpdateConfigAsync(int id, SlaConfigDto dto);
    }
}
