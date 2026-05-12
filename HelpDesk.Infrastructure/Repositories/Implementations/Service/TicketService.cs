using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Ticket;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
        private readonly IEmailQueue _emailQueue;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketService (IMapper mapper, IUnitOfWork unitOfWork , ICurrentUserProvider currentUserProvider , ISlaCalculatorService slaCalculator, IEmailQueue emailQueue,IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _slaCalculator = slaCalculator;
            _emailQueue = emailQueue;
            _config = config;
            _userManager = userManager; 
        }


        public async Task<ApiResponse<CreateTicketResponseDto>> CreateTicketAsync(CreateTicketDto dto)
        {
            var currentUserId = _currentUserProvider.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId)) return ApiResponse<CreateTicketResponseDto>.Failure("User not found.");

            var ticket = _mapper.Map<Ticket>(dto);
            ticket.Status = TicketStatus.Open;
            ticket.RaisedByUserId = !string.IsNullOrEmpty(dto.RaisedForUserId) ? dto.RaisedForUserId : currentUserId;

            // 1. Ask Identity for the user's full profile to get their Department
            var raisedByUser = await _userManager.FindByIdAsync(ticket.RaisedByUserId);

            // --- THE FIX: Assign the Department ID ---
            // If the user somehow doesn't have a department, fallback to '1' (General)
            ticket.DepartmentId = raisedByUser?.DepartmentId ?? 1;

            // --- SLA DEADLINE CALCULATION ---
            var slaConfigs = await _unitOfWork.SlaConfigs.GetAllAsync();
            var config = slaConfigs.FirstOrDefault(c => c.Priority == ticket.Priority);

            if (config != null)
            {
                ticket.SlaDeadline = await _slaCalculator.CalculateDeadlineAsync(DateTime.UtcNow, config.ResolutionHours);
            }
            else
            {
                ticket.SlaDeadline = DateTime.UtcNow.AddHours(24);
            }

            var targetEmail = raisedByUser?.Email ?? "admin@helpdesk.com";

            await _unitOfWork.Tickets.AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync(); // This will succeed now!

            var frontendUrl = _config["FrontendUrl"];
            var ticketUrl = $"{frontendUrl}/tickets/details/{ticket.Id}";

            var emailPayload = new EmailPayload
            {
                To = targetEmail,
                Subject = $"New Ticket Created: {ticket.Title}",
                Body = $"Hello,\n\nA new ticket (ID: {ticket.Id}) was just raised on your behalf.\n\nThank you,\nHelpDesk Team"
            };

            await _emailQueue.QueueEmailAsync(emailPayload);

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
            var currentUserRole = _currentUserProvider.GetCurrentUserRole();

            if (string.IsNullOrEmpty(currentUserId)) return ApiResponse<bool>.Failure("User not found.");

            var ticket = await _unitOfWork.Tickets.GetByIdAsync(dto.TicketId);
            if (ticket == null)
                return ApiResponse<bool>.Failure("Ticket not found");

            // --- FIX 1 (L06): Admin Bypass ---
            // If they are NOT an Admin, they MUST be the assigned agent to change the status.
            if (currentUserRole != "Admin" && ticket.AssignedToUserId != currentUserId)
            {
                return ApiResponse<bool>.Failure("You can only update the status of tickets assigned to you.");
            }

            // --- FIX 2 (L07): Restricted Statuses ---
            // Only Admins are allowed to Close or Reopen tickets.
            if ((dto.Status == TicketStatus.Closed || dto.Status == TicketStatus.Reopened) && currentUserRole != "Admin")
            {
                return ApiResponse<bool>.Failure("Only an Administrator can Close or Reopen a ticket.");
            }

            if (!Enum.IsDefined(typeof(TicketStatus), dto.Status))
                return ApiResponse<bool>.Failure("Invalid status value");

            var oldStatus = ticket.Status;

            ticket.Status = dto.Status;
            // --- PHASE 3: SMART SLA PAUSING (Gap L03) ---
            if (dto.Status == TicketStatus.OnHold)
            {
                // Freeze the timer!
                ticket.SlaPausedAt = DateTime.UtcNow;
            }
            else if (oldStatus == TicketStatus.OnHold && dto.Status == TicketStatus.InProgress)
            {
                // Unfreeze the timer and extend the deadline by the paused duration
                if (ticket.SlaPausedAt.HasValue)
                {
                    var pausedDuration = DateTime.UtcNow - ticket.SlaPausedAt.Value;
                    ticket.SlaDeadline = ticket.SlaDeadline.Add(pausedDuration);
                    ticket.SlaPausedAt = null;
                }
            }
            else if (dto.Status == TicketStatus.Reopened)
            {
                ticket.SlaPausedAt = null; // Clear any old pauses
                var slaConfigs = await _unitOfWork.SlaConfigs.GetAllAsync();
                var config = slaConfigs.FirstOrDefault(c => c.Priority == ticket.Priority);
                if (config != null)
                {
                    ticket.SlaDeadline = await _slaCalculator.CalculateDeadlineAsync(DateTime.UtcNow, config.ResolutionHours);
                }
            }

            await _unitOfWork.Tickets.UpdateAsync(ticket);
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
            if (ticket == null) return ApiResponse<bool>.Failure("Ticket not found");

            if (!Enum.IsDefined(typeof(TicketPriority), dto.Priority))
                return ApiResponse<bool>.Failure("Invalid priority value");

            ticket.Priority = dto.Priority;

            // --- PHASE 3: SLA RECALCULATION (Gap L04) ---
            // The Priority changed, so we must calculate a new strict deadline starting from NOW.
            var slaConfigs = await _unitOfWork.SlaConfigs.GetAllAsync();
            var config = slaConfigs.FirstOrDefault(c => c.Priority == ticket.Priority);

            if (config != null)
            {
                ticket.SlaDeadline = await _slaCalculator.CalculateDeadlineAsync(DateTime.UtcNow, config.ResolutionHours);
            }

            await _unitOfWork.Tickets.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Ticket priority and SLA deadline updated successfully");
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

        public async Task<ApiResponse<bool>> SubmitTicketFeedbackAsync(SubmitFeedbackDto dto)
        {
            var currentUserId = _currentUserProvider.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId)) return ApiResponse<bool>.Failure("User not found.");

            var ticket = await _unitOfWork.Tickets.GetByIdAsync(dto.TicketId);

            if (ticket == null)
                return ApiResponse<bool>.Failure("Ticket not found.");

            if (ticket.RaisedByUserId != currentUserId)
                return ApiResponse<bool>.Failure("You can only submit feedback for your own tickets.");

            if (ticket.Status != TicketStatus.Closed && ticket.Status != TicketStatus.Resolved)
                return ApiResponse<bool>.Failure("Feedback can only be submitted for resolved or closed tickets.");

            // PRD 11.2: Check for duplicate submissions (Max 1 per ticket)
            // Note: Assuming you added TicketFeedbacks to your IUnitOfWork
            var existingFeedback = await _unitOfWork.TicketFeedbacks.FindAsync(f => f.TicketId == dto.TicketId);
            if (existingFeedback.Any())
            {
                return ApiResponse<bool>.Failure("Feedback has already been submitted for this ticket.");
            }

            var feedback = new TicketFeedback
            {
                TicketId = dto.TicketId,
                CsatScore = dto.CsatScore,
                Comments = dto.Comments,
                SubmittedByUserId = currentUserId,
                CreatedDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };

            await _unitOfWork.TicketFeedbacks.AddAsync(feedback);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Thank you! Your feedback has been recorded.");
        }


    }
}
