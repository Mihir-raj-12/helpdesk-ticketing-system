using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Department;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentResponseDto>>>> GetAllDepartments()
        {
            var result = await _departmentService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DepartmentResponseDto>>> CreateDepartment([FromBody] CreateDepartmentDto dto)
        {
            var result = await _departmentService.CreateAsync(dto);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse<DepartmentResponseDto>>> UpdateDepartment([FromBody] UpdateDepartmentDto dto)
        {
            var result = await _departmentService.UpdateAsync(dto);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("deactivate/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateDepartment(int id)
        {
            var result = await _departmentService.DeactivateAsync(id);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}
