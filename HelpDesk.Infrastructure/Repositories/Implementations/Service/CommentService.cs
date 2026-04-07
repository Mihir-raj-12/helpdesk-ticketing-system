using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Comment;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class CommentService : ICommentService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;

        public CommentService (IMapper mapper, IUnitOfWork unitOfWork,ICurrentUserProvider currentUserProvider)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ApiResponse<CommentResponseDto>> AddCommentAsync(CreateCommentDto dto)
        {
            var currentUserId = _currentUserProvider.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId)) return ApiResponse<CommentResponseDto>.Failure("User not found.");

            var ticket = await _unitOfWork.Tickets.GetByIdAsync(dto.TicketId);
            if (ticket == null)
            {
                return ApiResponse<CommentResponseDto>.Failure("Ticket not found");
            }

            var comment = _mapper.Map<Comment>(dto);
            comment.UserId = currentUserId;

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            var SavedComment = await _unitOfWork.Comments.GetCommentsByTicketIdAsync(dto.TicketId);
            var newComment = SavedComment.LastOrDefault();

            var responseDto = _mapper.Map<CommentResponseDto>(newComment);
            return ApiResponse<CommentResponseDto>.Success(responseDto, "Comment added Successfully");

        }


        public async Task<ApiResponse<List<CommentResponseDto>>> GetCommentsByTicketIdAsync
            (int TicketId)
        {
            var currentUserID = _currentUserProvider.GetCurrentUserId();
            var currentUserRole = _currentUserProvider.GetCurrentUserRole();

            var ticket = await _unitOfWork.Tickets.GetByIdAsync(TicketId);

            if(ticket == null)
            {
                return ApiResponse<List<CommentResponseDto>>.Failure("Ticket not found");
            }


            if (currentUserRole == "RegularUser" && ticket.RaisedByUserId != currentUserID)
                return ApiResponse<List<CommentResponseDto>>.Failure("You do not have Access to this Ticket");

            if (currentUserRole == "SupportAgent" &&
                ticket.AssignedToUserId != currentUserID)
                return ApiResponse<List<CommentResponseDto>>
                    .Failure("You do not have access to this ticket");

            var comments = await _unitOfWork.Comments
              .GetCommentsByTicketIdAsync(TicketId);
            var responseDto = _mapper.Map<List<CommentResponseDto>>(comments);
            return ApiResponse<List<CommentResponseDto>>.Success(responseDto);
        }
    }


}
