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
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly ISlaCalculatorService _slaCalculator;

        public TicketService (IMapper mapper, IUnitOfWork unitOfWork , ICurrentUserProvider currentUserProvider , ISlaCalculatorService slaCalculator)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _slaCalculator = slaCalculator;
        }


        public async Task<ApiResponse<CreateTicketResponseDto>> CreateTicketAsync(CreateTicketDto dto)
        {
            var currentUserId = _currentUserProvider.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId)) return ApiResponse<CreateTicketResponseDto>.Failure("User not found.");

            var ticket = _mapper.Map<Ticket>(dto);
            ticket.Status = TicketStatus.Open;

            ticket.RaisedByUserId = !string.IsNullOrEmpty(dto.RaisedForUserId) ? dto.RaisedForUserId : currentUserId;

            // --- NEW: SLA DEADLINE CALCULATION ---

            // 1. Get all SLA configurations from the database
            var slaConfigs = await _unitOfWork.SlaConfigs.GetAllAsync();

            // 2. Find the specific rule for this ticket's priority
            var config = slaConfigs.FirstOrDefault(c => c.Priority == ticket.Priority);

            if (config != null)
            {
                // 3. Run the math engine to find out exactly when this ticket breaches!
                ticket.SlaDeadline = await _slaCalculator.CalculateDeadlineAsync(DateTime.UtcNow, config.ResolutionHours);
            }
            else
            {
                // Fallback safety net (just in case the SlaConfig table is empty)
                ticket.SlaDeadline = DateTime.UtcNow.AddHours(24);
            }

            await _unitOfWork.Tickets.AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync(); 

            var responseDto = new CreateTicketResponseDto
            {
                Id = ticket.Id,
                Status = ticket.Status
            };

            return ApiResponse<CreateTicketResponseDto>.Success(responseDto, "Ticket Created successfully");
        }

        public async Task<ApiResponse<List<TicketResponseDto>>> GetTicketsAsync()

        {
            var currentUserId = _currentUserProvider.GetCurrentUserId();
            var currentUserRole = _currentUserProvider.GetCurrentUserRole();

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
            int ticketId)
        {

            var currentUserId = _currentUserProvider.GetCurrentUserId();
            var currentUserRole = _currentUserProvider.GetCurrentUserRole();

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
            UpdateTicketStatusDto dto)
        {   
            var currentUserId = _currentUserProvider.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId)) return ApiResponse<bool>.Failure("User not found.");

            var ticket = await _unitOfWork.Tickets.GetByIdAsync(dto.TicketId);
            if (ticket == null)
                return ApiResponse<bool>.Failure("Ticket not found");

            // Only assigned agent can update status
            if (ticket.AssignedToUserId != currentUserId)
                return ApiResponse<bool>.Failure(
                    "You can only update status of tickets assigned to you");

            if (!Enum.IsDefined(typeof(TicketStatus), dto.Status))
                return ApiResponse<bool>.Failure("Invalid status value");

            var oldStatus = ticket.Status;

            ticket.Status = dto.Status;

            await _unitOfWork.Tickets.UpdateAsync(ticket, t => t.Status);

            //await _unitOfWork.AuditLogs.LogAsync(
            //    tableName: "Tickets",
            //    action: "UpdateStatus",
            //    performedByUserId: currentUserId,
            //    changes: new List<(string, string?, string?)>
            //    {
            //        ("Status", oldStatus.ToString(), dto.Status.ToString())
            //    });

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Ticket status updated successfully");
        }

        public async Task<ApiResponse<bool>> AssignTicketAsync(
            AssignTicketDto dto)
        {

            var currentUserId = _currentUserProvider.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId)) return ApiResponse<bool>.Failure("User not found.");

            var ticket = await _unitOfWork.Tickets.GetByIdAsync(dto.TicketId);
            if (ticket == null)
                return ApiResponse<bool>.Failure("Ticket not found");

            var oldAgentId = ticket.AssignedToUserId;
            ticket.AssignedToUserId = dto.AgentId;
            ticket.Status = TicketStatus.InProgress;

            await _unitOfWork.Tickets.UpdateAsync(ticket,
                 t => t.AssignedToUserId,
                 t => t.Status);

            // Audit log
            //await _unitOfWork.AuditLogs.LogAsync(
            //    tableName: "Tickets",
            //    action: "Assign",
            //    performedByUserId: currentUserId,
            //    changes: new List<(string, string?, string?)>
            //    {
            //        ("AssignedToUserId", oldAgentId, dto.AgentId),
            //        ("Status", "Open", "InProgress")
            //    });

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Ticket assigned successfully");
        }
        public async Task<ApiResponse<bool>> UpdateTicketPriorityAsync(
            UpdateTicketPriorityDto dto)
        {
            var currentUserId = _currentUserProvider.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId)) return ApiResponse<bool>.Failure("User not found.");


            var ticket = await _unitOfWork.Tickets.GetByIdAsync(dto.TicketId);
            if (ticket == null)
                return ApiResponse<bool>.Failure("Ticket not found");

        
            if (!Enum.IsDefined(typeof(TicketPriority), dto.Priority))
                return ApiResponse<bool>.Failure("Invalid priority value");

            var oldPriority = ticket.Priority;

  
            ticket.Priority = dto.Priority;

            await _unitOfWork.Tickets.UpdateAsync(ticket, t => t.Priority);

    
            //await _unitOfWork.AuditLogs.LogAsync(
            //    tableName: "Tickets",
            //    action: "UpdatePriority",
            //    performedByUserId: currentUserId,
            //    changes: new List<(string, string?, string?)>
            //    {
            //        ("Priority", oldPriority.ToString(), dto.Priority.ToString())
            //    });

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Ticket priority updated successfully");
        }

        public async Task<ApiResponse<TicketResponseDto>> EscalateTicketAsync(int ticketId, string reason, string currentUserId)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null) return ApiResponse<TicketResponseDto>.Failure("Ticket not found.");

            // Prevent double-escalation if it hasn't been acknowledged yet
            if (ticket.EscalationFlag && ticket.EscalationAcknowledgedAt == null)
            {
                return ApiResponse<TicketResponseDto>.Failure("Ticket is already escalated and awaiting Admin acknowledgment.");
            }

            // 1. The Priority Bump (PRD 10.3)
            if (ticket.Priority == TicketPriority.Low) ticket.Priority = TicketPriority.Medium;
            else if (ticket.Priority == TicketPriority.Medium) ticket.Priority = TicketPriority.High;
            else if (ticket.Priority == TicketPriority.High) ticket.Priority = TicketPriority.Critical;
            // (If it's already Critical, it just stays Critical)

            // 2. Set the Escalation Flags
            ticket.EscalationFlag = true;
            ticket.EscalationReason = reason;
            ticket.EscalatedAt = DateTime.UtcNow;
            ticket.EscalationAcknowledgedAt = null; // Reset this in case it was escalated previously

            // 3. The SLA Reset (PRD 10.3)
            // We must fetch the specific SLA rules for the NEW priority
            var slaConfigs = await _unitOfWork.SlaConfigs.GetAllAsync();
            var config = slaConfigs.FirstOrDefault(c => c.Priority == ticket.Priority);

            if (config != null)
            {
                // Recalculate the deadline starting from RIGHT NOW
                ticket.SlaDeadline = await _slaCalculator.CalculateDeadlineAsync(DateTime.UtcNow, config.ResolutionHours);
            }

            await _unitOfWork.Tickets.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            var responseDto = _mapper.Map<TicketResponseDto>(ticket);
            return ApiResponse<TicketResponseDto>.Success(responseDto, $"Ticket escalated to {ticket.Priority}.");
        }

        public async Task<ApiResponse<TicketResponseDto>> AcknowledgeEscalationAsync(int ticketId)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null) return ApiResponse<TicketResponseDto>.Failure("Ticket not found.");

            if (!ticket.EscalationFlag)
                return ApiResponse<TicketResponseDto>.Failure("This ticket is not currently escalated.");

            // PRD 10.3: "acknowledging records a timestamp and removes the escalation highlight, but the flag remains on the record"
            ticket.EscalationAcknowledgedAt = DateTime.UtcNow;

            // Notice we do NOT set EscalationFlag = false. 
            // In Angular, you will write: *ngIf="ticket.escalationFlag && !ticket.escalationAcknowledgedAt" to show the red warning!

            await _unitOfWork.Tickets.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            var responseDto = _mapper.Map<TicketResponseDto>(ticket);
            return ApiResponse<TicketResponseDto>.Success(responseDto, "Escalation acknowledged successfully.");
        }


    }
}
