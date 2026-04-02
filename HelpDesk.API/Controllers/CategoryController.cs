using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Category;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponseDto>>>> GetAllCategories()
        {
            var result = await _categoryService.GetAllCategoriesAsync();

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> CreateCategory([FromBody] CreateCategoryDto dto) 
        {

            var result = await _categoryService.CreateCategoryAsync(dto);

            if(!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);

        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> UpdateCategory([FromBody] UpdateCategoryDto request)
        {
            var response = await _categoryService.UpdateCategoryAsync(request.Id, request);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }


        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateCategory(int id)
        {
            var response = await _categoryService.DeactivateCategoryAsync(id);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }



    }
}
