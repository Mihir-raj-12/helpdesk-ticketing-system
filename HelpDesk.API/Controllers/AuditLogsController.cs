using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentLogs()
        {
            var response = await _auditLogService.GetRecentLogsAsync();

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportCsv([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Basic validation
            if (startDate > endDate)
                return BadRequest("Start date cannot be after end date.");

            var fileBytes = await _auditLogService.ExportLogsToCsvAsync(startDate, endDate);

            // Generate a dynamic filename based on the current date
            var fileName = $"AuditLogs_{DateTime.UtcNow:yyyyMMdd}.csv";

            // Return it as a downloadable CSV file
            return File(fileBytes, "text/csv", fileName);
        }
    }
}
