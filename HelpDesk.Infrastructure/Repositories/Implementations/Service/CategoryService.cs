using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Category;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class CategoryService : GenericService<CategoryResponseDto, Category>, ICategoryService
    {
        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserProvider currentUserProvider)
            : base(mapper, unitOfWork, currentUserProvider, unitOfWork.Categories) // Pass UnitOfWork.Categories to the base class!
        {
        }

        // We only need to write the custom Update logic, because the GenericService handles the rest!
        public async Task<ApiResponse<bool>> UpdateCategoryAsync(UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.Id);
            if (category == null)
            {
                return ApiResponse<bool>.Failure("Category Not Found");
            }

            category.Name = dto.Name;

            // Notice how we use our new partial update feature here!
            await _unitOfWork.Categories.UpdateAsync(category, c => c.Name);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Category Updated Successfully");
        }
    }
}
