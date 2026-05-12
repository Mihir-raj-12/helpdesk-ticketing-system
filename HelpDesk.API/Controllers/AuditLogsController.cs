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
            // Force the endDate to include the entire day (up to 23:59:59)
            // This prevents the "Midnight Trap" where logs from later in the afternoon get excluded!
            var actualEndDate = endDate.Date.AddDays(1).AddTicks(-1);

            // Basic validation
            if (startDate > actualEndDate)
                return BadRequest("Start date cannot be after end date.");

            var fileBytes = await _auditLogService.ExportLogsToCsvAsync(startDate, actualEndDate);

            // Generate a dynamic filename based on the current date
            var fileName = $"AuditLogs_{DateTime.UtcNow:yyyyMMdd}.csv";

            // Return it as a downloadable CSV file
            return File(fileBytes, "text/csv", fileName);
        }
    }
}
