using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface INotificationPreferenceService
    {
        Task<ApiResponse<NotificationPreferenceDto>> GetMyPreferencesAsync();
        Task<ApiResponse<NotificationPreferenceDto>> UpdateMyPreferencesAsync(NotificationPreferenceDto dto);
    }
}
