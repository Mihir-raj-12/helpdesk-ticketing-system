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
           

            var response = await _dashboardService.GetDashboardStatusAsync();

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("export/urgent")]
        public async Task<IActionResult> ExportUrgentTickets()
        {
            try
            {
                var export = await _dashboardService.ExportActionableTicketsCsvAsync();

                // This tells the browser "Download this file immediately!"
                return File(export.FileContents, export.ContentType, export.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Failure($"Export failed: {ex.Message}"));
            }
        }
    }
}
