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
            : base(mapper, unitOfWork, currentUserProvider, unitOfWork.Categories) 
        {
        }

        public async Task<ApiResponse<bool>> UpdateCategoryAsync(UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.Id);
            if (category == null)
            {
                return ApiResponse<bool>.Failure("Category Not Found");
            }

            category.Name = dto.Name;

            await _unitOfWork.Categories.UpdateAsync(category, c => c.Name);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Category Updated Successfully");
        }

        public override async Task<ApiResponse<bool>> DeactivateAsync(int id)
        {
            // PRD 14.3: Prevent deactivation if there are active tickets
            // Check if any tickets use this category AND are not Closed/Resolved
            var activeTickets = await _unitOfWork.Tickets.FindAsync(t =>
                t.CategoryId == id &&
                t.Status != Core.Enums.TicketStatus.Closed &&
                t.Status != Core.Enums.TicketStatus.Resolved);

            if (activeTickets.Any())
            {
                return ApiResponse<bool>.Failure($"Cannot deactivate this category. There are {activeTickets.Count()} active tickets currently using it.");
            }

            // If safe, let the generic service handle the soft delete!
            return await base.DeactivateAsync(id);
        }
    }
}
