using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Ticket;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class TicketService : ITicketService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public TicketService (IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }


        public async Task<ApiResponse<TicketResponseDto>> CreateTicketAsync (
            CreateTicketDto dto , string currentUserId
            )
        {
            var ticket = _mapper.Map<Ticket>(dto);
            ticket.Status = TicketStatus.Open;

            ticket.RaisedByUserId = !string.IsNullOrEmpty(dto.RaisedForUserId) ? dto.RaisedForUserId : currentUserId;


            await _unitOfWork.Tickets.AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            var savedTicket = await _unitOfWork.Tickets.GetTicketWithDetailsAsync(ticket.Id);
            var responseDto = _mapper.Map<TicketResponseDto>(savedTicket);

            return ApiResponse<TicketResponseDto>.Success(responseDto ,"Ticket Created successfully");

        }

        public async Task<ApiResponse<List<TicketResponseDto>>> GetTicketsAsync(
           string currentUserId, string currentUserRole)
        {
            IEnumerable<Ticket> tickets;

            if (currentUserRole == "Admin")
                tickets = await _unitOfWork.Tickets.GetAllTicketsWithDetailsAsync();
            else if (currentUserRole == "SupportAgent")
                tickets = await _unitOfWork.Tickets.GetTicketsByAgentIdAsync(currentUserId);
            else
                tickets = await _unitOfWork.Tickets.GetTicketsByUserIdAsync(currentUserId);

            var responseDto = _mapper.Map<List<TicketResponseDto>>(tickets);
            return ApiResponse<List<TicketResponseDto>>.Success(responseDto);
        }

        public async Task<ApiResponse<TicketResponseDto>> GetTicketByIdAsync(
            int ticketId, string currentUserId, string currentUserRole)
        {
            var ticket = await _unitOfWork.Tickets.GetTicketWithDetailsAsync(ticketId);
            if (ticket == null)
                return ApiResponse<TicketResponseDto>.Failure("Ticket not found");

            if (currentUserRole == "RegularUser" &&
                ticket.RaisedByUserId != currentUserId)
                return ApiResponse<TicketResponseDto>
                    .Failure("You do not have access to this ticket");

            if (currentUserRole == "SupportAgent" &&
                ticket.AssignedToUserId != currentUserId)
                return ApiResponse<TicketResponseDto>
                    .Failure("You do not have access to this ticket");

            var responseDto = _mapper.Map<TicketResponseDto>(ticket);
            return ApiResponse<TicketResponseDto>.Success(responseDto);
        }

        public async Task<ApiResponse<bool>> UpdateTicketStatusAsync(
            int ticketId, UpdateTicketStatusDto dto, string currentUserId)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null)
                return ApiResponse<bool>.Failure("Ticket not found");

            // Only assigned agent can update status
            if (ticket.AssignedToUserId != currentUserId)
                return ApiResponse<bool>.Failure(
                    "You can only update status of tickets assigned to you");

            if (!Enum.TryParse<TicketStatus>(dto.Status, out var newStatus))
                return ApiResponse<bool>.Failure("Invalid status value");

            var oldStatus = ticket.Status.ToString();
            ticket.Status = newStatus;

            await _unitOfWork.Tickets.UpdateAsync(ticket);

            // Audit log
            await _unitOfWork.AuditLogs.LogAsync(
                tableName: "Tickets",
                action: "UpdateStatus",
                performedByUserId: currentUserId,
                changes: new List<(string, string?, string?)>
                {
                    ("Status", oldStatus, newStatus.ToString())
                });

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Ticket status updated successfully");
        }

        public async Task<ApiResponse<bool>> AssignTicketAsync(
            int ticketId, AssignTicketDto dto, string currentUserId)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null)
                return ApiResponse<bool>.Failure("Ticket not found");

            var oldAgentId = ticket.AssignedToUserId;
            ticket.AssignedToUserId = dto.AgentId;
            ticket.Status = TicketStatus.InProgress;

            await _unitOfWork.Tickets.UpdateAsync(ticket);

            // Audit log
            await _unitOfWork.AuditLogs.LogAsync(
                tableName: "Tickets",
                action: "Assign",
                performedByUserId: currentUserId,
                changes: new List<(string, string?, string?)>
                {
                    ("AssignedToUserId", oldAgentId, dto.AgentId),
                    ("Status", "Open", "InProgress")
                });

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Ticket assigned successfully");
        }
        public async Task<ApiResponse<bool>> UpdateTicketPriorityAsync(
            int ticketId, UpdateTicketStatusDto dto, string currentUserId)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null)
                return ApiResponse<bool>.Failure("Ticket not found");

            if (!Enum.TryParse<TicketPriority>(dto.Status, out var newPriority))
                return ApiResponse<bool>.Failure("Invalid priority value");

            var oldPriority = ticket.Priority.ToString();
            ticket.Priority = newPriority;

            await _unitOfWork.Tickets.UpdateAsync(ticket);

            // Audit log
            await _unitOfWork.AuditLogs.LogAsync(
                tableName: "Tickets",
                action: "UpdatePriority",
                performedByUserId: currentUserId,
                changes: new List<(string, string?, string?)>
                {
                    ("Priority", oldPriority, newPriority.ToString())
                });

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Ticket priority updated successfully");
        }


    }
}
