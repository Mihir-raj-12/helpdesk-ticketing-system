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
    public class SlaConfigsController : ControllerBase
    {
        private readonly ISlaConfigService _slaConfigService;

        public SlaConfigsController(ISlaConfigService slaConfigService)
        {
            _slaConfigService = slaConfigService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _slaConfigService.GetAllConfigsAsync();

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SlaConfigDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _slaConfigService.UpdateConfigAsync(id, dto);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
