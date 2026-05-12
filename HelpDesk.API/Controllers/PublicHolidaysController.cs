using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Holidays;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Strictly Admin Only!
    public class PublicHolidaysController : ControllerBase
    {
        private readonly IPublicHolidayService _holidayService;

        public PublicHolidaysController(IPublicHolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PublicHolidayResponseDto>>>> GetAll()
        {
            var result = await _holidayService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PublicHolidayResponseDto>>> Create([FromBody] CreatePublicHolidayDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _holidayService.CreateAsync(dto);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            // We use Deactivate from the Generic Service to soft-delete the holiday
            var result = await _holidayService.DeactivateAsync(id);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }
    }
}
