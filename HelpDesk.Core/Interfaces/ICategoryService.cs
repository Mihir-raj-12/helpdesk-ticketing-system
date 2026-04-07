using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ICategoryService : IGenericService<CategoryResponseDto>
    {
        // We only need to define custom methods here (like your specific Update method)
        Task<ApiResponse<bool>> UpdateCategoryAsync(UpdateCategoryDto dto);
    }
}
