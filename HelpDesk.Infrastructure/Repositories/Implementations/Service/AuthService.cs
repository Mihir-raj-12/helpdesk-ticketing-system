using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Auth;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider)
        {
            _userManager = userManager;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var ipAddress = _currentUserProvider.GetClientIpAddress() ?? "Unknown IP";
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                await LogAuthEventAsync("Login Failed", dto.Email, null, ipAddress, "Invalid email");
                return ApiResponse<LoginResponseDto>.Failure("Invalid email or password");
            }

            if (!user.IsActive)
            {
                await LogAuthEventAsync("Login Failed", dto.Email, user, ipAddress, "Account deactivated");
                return ApiResponse<LoginResponseDto>.Failure("Your account has been deactivated");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                await LogAuthEventAsync("Login Failed", dto.Email, user, ipAddress, "Invalid password");
                return ApiResponse<LoginResponseDto>.Failure("Invalid email or password");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            var token = GenerateJwtToken(user, role);

            // PRD 8.2: Log successful login
            await LogAuthEventAsync("Login Success", dto.Email, user, ipAddress, "User logged in successfully");

            return ApiResponse<LoginResponseDto>.Success(new LoginResponseDto { Token = token }, "Login successful");
        }

        // --- NEW: LOGOUT LOGIC ---
        public async Task<ApiResponse<bool>> LogoutAsync()
        {
            var userId = _currentUserProvider.GetCurrentUserId();
            var ipAddress = _currentUserProvider.GetClientIpAddress() ?? "Unknown IP";

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                // PRD 8.2: Log successful logout
                await LogAuthEventAsync("Logout", user?.Email ?? "Unknown", user, ipAddress, "User logged out");
            }

            // Since JWTs are stateless, actual "logout" happens on the frontend by deleting the token.
            return ApiResponse<bool>.Success(true, "Logged out successfully");
        }

        // --- NEW: MANUAL AUDIT LOGGING ---
        private async Task LogAuthEventAsync(string action, string email, ApplicationUser? user, string ipAddress, string notes)
        {
            var role = "Unknown";
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                role = roles.FirstOrDefault() ?? "Unknown";
            }

            var log = new AuditLog
            {
                TableName = "Authentication", // Fictional table name to satisfy PRD categorization
                EntityId = user?.Id ?? "None",
                Action = action,
                PerformedByUserId = user?.Id,
                ActorName = user?.FullName ?? email,
                ActorEmail = email,
                ActorRole = role,
                IpAddress = ipAddress,
                AdditionalNotes = notes,
                PerformedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.AuditLogs.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }

        private string GenerateJwtToken(ApplicationUser user, string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;
            var expiryInDays = jwtSettings.GetValue<int>("ExpiryInDays");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expiryInDays),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
