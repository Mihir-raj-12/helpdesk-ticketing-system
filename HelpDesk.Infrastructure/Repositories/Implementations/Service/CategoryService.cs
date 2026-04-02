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
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<CategoryResponseDto>> CreateCategoryAsync (CreateCategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            var responseDto = _mapper.Map<CategoryResponseDto>(category);
            return ApiResponse<CategoryResponseDto>.Success(responseDto,"Category Created SuccessFully");

        }

        public async Task<ApiResponse<List<CategoryResponseDto>>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var responseDto =_mapper.Map<List<CategoryResponseDto>>(categories);
            return ApiResponse<List<CategoryResponseDto>>.Success(responseDto);
        }


        public async Task<ApiResponse<bool>> UpdateCategoryAsync(int id , UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if(category == null)
            {
                return ApiResponse<bool>.Failure("Category Not Found");
            }

            category.Name = dto.Name;
            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true,"Category Updated Successfully");
        }

        public async Task<ApiResponse<bool>> DeactivateCategoryAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return ApiResponse<bool>.Failure("category Not Found");

            }

            await _unitOfWork.Categories.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Category Deactivated Successfully");

        }
    }
}
