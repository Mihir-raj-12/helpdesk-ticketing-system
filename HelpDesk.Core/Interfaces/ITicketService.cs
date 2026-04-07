using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ITicketService
    {
        Task<ApiResponse<TicketResponseDto>> CreateTicketAsync(CreateTicketDto dto);
        Task<ApiResponse<List<TicketResponseDto>>> GetTicketsAsync();
        Task<ApiResponse<TicketResponseDto>> GetTicketByIdAsync(int ticketId);
        Task<ApiResponse<bool>> UpdateTicketStatusAsync(UpdateTicketStatusDto dto);
        Task<ApiResponse<bool>> AssignTicketAsync(AssignTicketDto dto);
        Task<ApiResponse<bool>> UpdateTicketPriorityAsync(UpdateTicketPriorityDto dto);
    }
}
