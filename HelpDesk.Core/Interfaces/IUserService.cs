using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IUserService

    {
        Task<ApiResponse<UserResponseDto>> CreateUserAsync(CreateUserDto dto);
        Task<ApiResponse<List<UserResponseDto>>> GetAllUserAsync ();

        Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(string id);

        Task<ApiResponse<bool>> UpdateUserRoleAsync (UpdateUserRoleDto dto);
         Task<ApiResponse<bool>> DeactivateUserAsync (string id);

        Task<ApiResponse<List<UserResponseDto>>> GetSupportAgentsAsync();

    }
}
