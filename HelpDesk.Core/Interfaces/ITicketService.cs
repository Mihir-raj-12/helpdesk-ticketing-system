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
        Task<ApiResponse<TicketResponseDto>> CreateTicketAsync(
            CreateTicketDto dto, string currentUserId);
        Task<ApiResponse<List<TicketResponseDto>>> GetTicketsAsync(
            string currentUserId, string currentUserRole);
        Task<ApiResponse<TicketResponseDto>> GetTicketByIdAsync(
            int ticketId, string currentUserId, string currentUserRole);
        Task<ApiResponse<bool>> UpdateTicketStatusAsync(
            int ticketId, UpdateTicketStatusDto dto, string currentUserId);
        Task<ApiResponse<bool>> AssignTicketAsync(
            int ticketId, AssignTicketDto dto, string currentUserId);
        Task<ApiResponse<bool>> UpdateTicketPriorityAsync(
            int ticketId, UpdateTicketStatusDto dto, string currentUserId);
    }
}
