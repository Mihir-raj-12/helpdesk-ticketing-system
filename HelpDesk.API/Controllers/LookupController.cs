using HelpDesk.Core.Common;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public LookupController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [HttpGet("statuses")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetStatuses()
        {
            var result = await _lookupService.GetStatusesAsync();
            return Ok(result);
        }

        [HttpGet("priorities")]
        public async Task<ActionResult<ApiResponse<List<object
            >>>> GetPriorities()
        {
            var result = await _lookupService.GetPrioritiesAsync();
            return Ok(result);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetRoles()
        {
            var result = await _lookupService.GetRolesAsync();
            return Ok(result);
        }


    }
}
