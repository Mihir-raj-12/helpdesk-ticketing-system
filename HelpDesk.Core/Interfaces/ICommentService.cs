using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ICommentService
    {
        Task<ApiResponse<CommentResponseDto>> AddCommentAsync(
            CreateCommentDto dto, string currentUserId);
        Task<ApiResponse<List<CommentResponseDto>>> GetCommentsByTicketIdAsync(
            int ticketId, string currentUserId, string currentUserRole);
    }
}
