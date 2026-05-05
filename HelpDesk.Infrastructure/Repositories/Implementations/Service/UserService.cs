using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.User;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        private readonly RoleManager<IdentityRole> _roleManager; // NEW
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork; // NEW
        private readonly IEmailQueue _emailQueue; // NEW

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEmailQueue emailQueue)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _emailQueue = emailQueue;
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
                CreatedDate = DateTime.UtcNow,
                DepartmentId = dto.DepartmentId
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<UserResponseDto>.Failure(errors);
            }


            await _userManager.AddToRoleAsync(user, dto.Role.ToString());

            var responseDto = _mapper.Map<UserResponseDto>(user);
            responseDto.Role = dto.Role.ToString();
            return ApiResponse<UserResponseDto>.Success(
                responseDto, "User created successfully"); 
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetAllUserAsync()
        {
            var users = await _userManager.Users
                .Include(u=>u.Department)
                .Where(u => u.IsActive).ToListAsync();

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
            var user = await _userManager.Users.Include(u=>u.DepartmentId).FirstOrDefaultAsync(u => u.Id == id);

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

            var result = await _userManager.AddToRoleAsync(user, dto.NewRole.ToString());
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

        public async Task<ApiResponse<List<UserResponseDto>>> GetSupportAgentsAsync()
        {
            
            var agents = await _userManager.GetUsersInRoleAsync("SupportAgent");

            var agentIds = agents.Select(a => a.Id).ToList();


            var activeAgentsWithDepts = await _userManager.Users
        .Include(u => u.Department)
        .Where(u => agentIds.Contains(u.Id) && u.IsActive)
        .ToListAsync();

          
            var responseDto = new List<UserResponseDto>();
            foreach (var agent in activeAgentsWithDepts)
            {
                var dto = _mapper.Map<UserResponseDto>(agent);
                dto.Role = "SupportAgent";
                responseDto.Add(dto);
            }

            return ApiResponse<List<UserResponseDto>>.Success(responseDto);
        }

        public async Task<ApiResponse<BulkImportResultDto>> BulkImportUsersAsync(Stream csvStream)
        {
            var result = new BulkImportResultDto();
            var allDepartments = await _unitOfWork.Departments.GetAllAsync();

            using var reader = new StreamReader(csvStream);

            // 1. Read and validate the header row
            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine) || !headerLine.Contains("Email"))
            {
                return ApiResponse<BulkImportResultDto>.Failure("Invalid CSV format. Expected Header: Name, Email, Department, Role");
            }

            int lineNumber = 1;

            // 2. Process each line
            while (!reader.EndOfStream)
            {
                lineNumber++;
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                result.TotalProcessed++;
                var columns = line.Split(',');

                if (columns.Length < 4)
                {
                    result.Errors.Add($"Row {lineNumber}: Missing columns. Expected 4, found {columns.Length}.");
                    continue;
                }

                var fullName = columns[0].Trim();
                var email = columns[1].Trim();
                var departmentName = columns[2].Trim();
                var roleName = columns[3].Trim();

                // 3. Duplicate Check (PRD 14.6: skip duplicates, do not overwrite)
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    result.SkippedDuplicates++;
                    result.Errors.Add($"Row {lineNumber}: Email '{email}' already exists. Skipped.");
                    continue;
                }

                // 4. Role Validation
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    result.Errors.Add($"Row {lineNumber}: Role '{roleName}' does not exist.");
                    continue;
                }

                // 5. Department Mapping (Fallback to 'General' if not found)
                var department = allDepartments.FirstOrDefault(d => d.Name.Equals(departmentName, StringComparison.OrdinalIgnoreCase));
                int departmentId = department?.Id ?? 1; // 1 is the 'General' fallback ID

                // 6. Create the User
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    DepartmentId = departmentId,
                    EmailConfirmed = true, // Auto-confirm for internal imports
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                // Generate a temporary password
                var tempPassword = "TempPassword123!";
                var createResult = await _userManager.CreateAsync(newUser, tempPassword);

                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, roleName);
                    result.SuccessfullyCreated++;

                    // 7. Fire off the Welcome Email to our Background Queue!
                    var emailPayload = new EmailPayload
                    {
                        To = email,
                        Subject = "Welcome to the HelpDesk System",
                        Body = $"Hello {fullName},\n\nYour account has been created.\nRole: {roleName}\nTemporary Password: {tempPassword}\n\nPlease log in and change your password immediately."
                    };
                    await _emailQueue.QueueEmailAsync(emailPayload);
                }
                else
                {
                    result.Errors.Add($"Row {lineNumber}: Failed to create user '{email}'. Reason: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            return ApiResponse<BulkImportResultDto>.Success(result, "Bulk import completed.");
        }
    }
}
