using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.User;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;


        public UserService ( UserManager<ApplicationUser> userManager , IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ApiResponse<UserResponseDto>> CreateUserAsync (CreateUserDto dto)
        {
            var exisitingUser = await _userManager.FindByEmailAsync(dto.Email);
            if(exisitingUser != null )
            {
                return ApiResponse<UserResponseDto>.Failure("Email already exists");   
            }

            var user =  new ApplicationUser
           {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<UserResponseDto>.Failure(errors);
            }


            await _userManager.AddToRoleAsync(user, dto.Role);

            var responseDto = _mapper.Map<UserResponseDto>(user);
            responseDto.Role = dto.Role;
            return ApiResponse<UserResponseDto>.Success(
                responseDto, "User created successfully");
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetAllUserAsync()
        {
            var users = _userManager.Users.Where(u => u.IsActive).ToList();

            var responseDto = new List<UserResponseDto>();
            foreach (var user in users)
            {
                var roles =await _userManager.GetRolesAsync(user);
                var dto = _mapper.Map<UserResponseDto>(user);
                dto.Role = roles.FirstOrDefault() ?? string.Empty;
                responseDto.Add(dto);
            }
            return ApiResponse<List<UserResponseDto>>.Success(responseDto);
        }

        public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.IsActive)
                return ApiResponse<UserResponseDto>.Failure("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            var responseDto = _mapper.Map<UserResponseDto>(user);
            responseDto.Role = roles.FirstOrDefault() ?? string.Empty;
            return ApiResponse<UserResponseDto>.Success(responseDto);
        }

        public async Task<ApiResponse<bool>> UpdateUserRoleAsync(UpdateUserRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null || !user.IsActive)
                return ApiResponse<bool>.Failure("User not found");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await _userManager.AddToRoleAsync(user, dto.NewRole);
            if (!result.Succeeded)
                return ApiResponse<bool>.Failure("Failed to update role");

            return ApiResponse<bool>.Success(true, "User role updated successfully");
        }

        public async Task<ApiResponse<bool>> DeactivateUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.IsActive)
                return ApiResponse<bool>.Failure("User not found");

            user.IsActive = false;
            await _userManager.UpdateAsync(user);
            return ApiResponse<bool>.Success(true, "User deactivated successfully");
        }
    }
}
