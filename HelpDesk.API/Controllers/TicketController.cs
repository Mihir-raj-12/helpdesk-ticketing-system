using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Ticket;
using HelpDesk.Core.Entities;
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
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse<CreateTicketResponseDto>>> CreateTicket([FromBody] CreateTicketDto dto)
        {


            var result = await _ticketService.CreateTicketAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);

        }

        [HttpGet]

        public async Task<ActionResult<ApiResponse<IEnumerable<TicketResponseDto>>>> GetAllTickets()
        {
           
            var result = await _ticketService.GetTicketsAsync();
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

        [HttpGet("getById")]
        public async Task<ActionResult<ApiResponse<TicketResponseDto>>> GetTicketById([FromQuery] int id)
        {
            var result = await _ticketService.GetTicketByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }


        [HttpPut("status")]
        [Authorize(Roles = "Admin, SupportAgent")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTicketStatus([FromBody] UpdateTicketStatusDto dto)
        {
            var result = await _ticketService.UpdateTicketStatusAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);


        }

        [HttpPut("assign")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignTicket([FromBody] AssignTicketDto dto)
        {
           
            var result = await _ticketService.AssignTicketAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

        [HttpPut("priority")]
        [Authorize(Roles = "Admin, SupportAgent")]

        public async Task<ActionResult<ApiResponse<bool>>> UpdateTicketPriority( [FromBody] UpdateTicketPriorityDto dto)
        {
          
            var result = await _ticketService.UpdateTicketPriorityAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

        [HttpPost("escalate")]
        [Authorize(Roles = "Admin,SupportAgent")] // Users cannot escalate their own tickets!
        public async Task<IActionResult> EscalateTicket([FromQuery] int id, [FromBody] EscalateTicketDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Grab the user ID from the JWT token
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var response = await _ticketService.EscalateTicketAsync(id, dto.Reason, userId);
            if (!response.IsSuccess) return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("acknowledge-escalation")]
        [Authorize(Roles = "Admin")] // PRD limits acknowledgment strictly to Admins
        public async Task<IActionResult> AcknowledgeEscalation([FromQuery] int id)
        {
            var response = await _ticketService.AcknowledgeEscalationAsync(id);
            if (!response.IsSuccess) return BadRequest(response);

            return Ok(response);
        }

    }




    
}
