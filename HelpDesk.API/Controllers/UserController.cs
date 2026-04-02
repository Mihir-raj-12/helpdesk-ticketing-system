using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.User;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> CreateUser([FromBody] CreateUserDto dto)
        {
            var result = await _userService.CreateUserAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet]

        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDto>>>> GetAllUsers()
        {
            var result = await _userService.GetAllUserAsync();

            return Ok(result);

        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUserById(string id)
        {
            var response = await _userService.GetUserByIdAsync(id);

            if (!response.IsSuccess)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpPut("{id}/deactivate")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateUser(string id)
        {
            var response = await _userService.DeactivateUserAsync(id);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("role")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateRole([FromBody] UpdateUserRoleDto request)
        {
          
            var response = await _userService.UpdateUserRoleAsync(request);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

    }
}
