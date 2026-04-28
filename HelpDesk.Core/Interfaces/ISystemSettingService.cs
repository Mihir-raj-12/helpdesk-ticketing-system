using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ISystemSettingService
    {
        Task<ApiResponse<SystemSettingDto>> GetSettingsAsync();
        Task<ApiResponse<SystemSettingDto>> UpdateSettingsAsync(SystemSettingDto dto);
    }
}
