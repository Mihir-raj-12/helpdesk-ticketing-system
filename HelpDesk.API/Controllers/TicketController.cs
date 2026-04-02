using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Ticket;
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
        public async Task<ActionResult<ApiResponse<TicketResponseDto>>> CreateTicket([FromBody] CreateTicketDto dto)
        {

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(ApiResponse<TicketResponseDto>.Failure("UserId is not found from tocken"));
            }


            var result = await _ticketService.CreateTicketAsync(dto, currentUserId);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);

        }

        [HttpGet]

        public async Task<ActionResult<ApiResponse<IEnumerable<TicketResponseDto>>>> GetAllTickets()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(currentUserRole))
            {
                return Unauthorized(ApiResponse<IEnumerable<TicketResponseDto>>.Failure("UserId or Role is not found from token"));
            }
            var result = await _ticketService.GetTicketsAsync(currentUserId, currentUserRole);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<ApiResponse<TicketResponseDto>>> GetTicketById(int Id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(currentUserRole))
            {
                return Unauthorized(ApiResponse<TicketResponseDto>.Failure("UserId or Role is not found from token"));
            }
            var result = await _ticketService.GetTicketByIdAsync(Id, currentUserId, currentUserRole);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


        [HttpPut("{Id}/status")]
        [Authorize(Roles = "Admin, SupportAgent")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTicketStatus(int Id, [FromBody] UpdateTicketStatusDto dto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(ApiResponse<bool>.Failure("UserId is not found from token"));
            }
            var result = await _ticketService.UpdateTicketStatusAsync(Id, dto, currentUserId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);


        }

        [HttpPut("{Id}/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignTicket(int Id, [FromBody] AssignTicketDto dto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(ApiResponse<bool>.Failure("UserId is not found from token"));
            }
            var result = await _ticketService.AssignTicketAsync(Id, dto, currentUserId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

        [HttpPut("{Id}/priority")]
        [Authorize(Roles = "Admin, SupportAgent")]

        public async Task<ActionResult<ApiResponse<bool>>> UpdateTicketPriority(int Id, [FromBody] UpdateTicketStatusDto dto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(ApiResponse<bool>.Failure("UserId is not found from token"));
            }
            var result = await _ticketService.UpdateTicketPriorityAsync(Id, dto, currentUserId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

    }




    
}
