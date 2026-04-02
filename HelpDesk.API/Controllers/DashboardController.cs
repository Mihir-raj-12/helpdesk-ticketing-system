using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Dashboard;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelpDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<DashboardResponseDto>>> GetStats()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(currentUserRole))
            {
                return Unauthorized(ApiResponse<DashboardResponseDto>.Failure("Invalid token claims."));
            }

            var response = await _dashboardService.GetDashboardStatusAsync(currentUserId, currentUserRole);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
