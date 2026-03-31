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
    internal class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager; 
        private readonly IConfiguration _configuration;

        public AuthService (UserManager<ApplicationUser> userManager , IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return ApiResponse<LoginResponseDto>.Failure("Invalid email or password");

            if (!user.IsActive)
                return ApiResponse<LoginResponseDto>.Failure("Your account has been deactivated");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
                return ApiResponse<LoginResponseDto>.Failure("Invalid email or password");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            var token = GenerateJwtToken(user, role);

            var response = new LoginResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = role,
                ExpiresAt = DateTime.UtcNow.AddDays(
                    _configuration.GetValue<int>("JwtSettings:ExpiryInDays"))
            };

            return ApiResponse<LoginResponseDto>.Success(response, "Login successful");
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
