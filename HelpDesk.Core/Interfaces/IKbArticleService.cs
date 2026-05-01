using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.KnowledgeBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IKbArticleService
    {
        Task<ApiResponse<KbArticleResponseDto>> GetArticleByIdAsync(int id);
        Task<ApiResponse<IEnumerable<KbArticleResponseDto>>> GetAllPublishedAsync();
        Task<ApiResponse<KbArticleResponseDto>> CreateArticleAsync(CreateKbArticleDto dto);
        Task<ApiResponse<KbArticleResponseDto>> UpdateArticleAsync(int id, UpdateKbArticleDto dto);
    }
}
