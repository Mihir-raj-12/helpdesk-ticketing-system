using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<ApiResponse<CategoryResponseDto>> CreateCategoryAsync(CreateCategoryDto dto);
        Task<ApiResponse<List<CategoryResponseDto>>> GetAllCategoriesAsync();
        Task<ApiResponse<bool>> UpdateCategoryAsync(int id, CreateCategoryDto dto);
        Task<ApiResponse<bool>> DeactivateCategoryAsync(int id);
    }
}
