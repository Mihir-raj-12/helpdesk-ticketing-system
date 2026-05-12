using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Notifications;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Any logged-in user can manage their notifications
    public class NotificationPreferencesController : ControllerBase
    {
        private readonly INotificationPreferenceService _preferenceService;

        public NotificationPreferencesController(INotificationPreferenceService preferenceService)
        {
            _preferenceService = preferenceService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> GetMyPreferences()
        {
            var response = await _preferenceService.GetMyPreferencesAsync();
            if (!response.IsSuccess) return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("me")]
        public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> UpdateMyPreferences([FromBody] NotificationPreferenceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _preferenceService.UpdateMyPreferencesAsync(dto);
            if (!response.IsSuccess) return BadRequest(response);

            return Ok(response);
        }
    }
}
