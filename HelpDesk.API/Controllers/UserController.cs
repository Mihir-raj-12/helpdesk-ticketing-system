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
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService) {
            _userService = userService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDto>>>> GetAllUsers()
        {
            var result = await _userService.GetAllUserAsync();

            return Ok(result);

        }


        [HttpGet("getById")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUserById([FromQuery] string id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPut("DeactivateUser")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateUser([FromBody]string id)
        {
            var response = await _userService.DeactivateUserAsync(id);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("role")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateRole([FromBody] UpdateUserRoleDto request)
        {
          
            var response = await _userService.UpdateUserRoleAsync(request);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("agents")]
        [Authorize(Roles = "Admin, SupportAgent")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDto>>>> GetSupportAgents()
        {
            var result = await _userService.GetSupportAgentsAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

    }
}
