using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Settings;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SettingsController : ControllerBase
    {
        private readonly ISystemSettingService _settingService;

       public SettingsController(ISystemSettingService settingService)
       {
            _settingService = settingService;
       }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<SystemSettingDto>>> GetSettings()
        {
            var result = await _settingService.GetSettingsAsync();
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse<SystemSettingDto>>> UpdateSettings([FromBody] SystemSettingDto dto)
        {
            var result = await _settingService.UpdateSettingsAsync(dto);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

    }
}
