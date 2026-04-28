using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Department;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class DepartmentService : IDepartmentService
    {

        private readonly ApplicationDbContext _context;


        public DepartmentService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<ApiResponse<IEnumerable<DepartmentResponseDto>>> GetAllAsync()
        {
            var departments = await _context.Departments
                .Include(d => d.DepartmentHead)
                .Include(d => d.Users)
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    IsActive = d.IsActive,
                    DepartmentHeadId = d.DepartmentHeadId,
                    // Send the string name so Angular doesn't have to look it up!
                    DepartmentHeadName = d.DepartmentHead != null ? d.DepartmentHead.FullName : "Unassigned",
                    // PRD Requirement: Count active users
                    ActiveUserCount = d.Users.Count(u => u.IsActive)
                })
                .ToListAsync();

            return ApiResponse<IEnumerable<DepartmentResponseDto>>.Success(departments);
        }

        public async Task<ApiResponse<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto)
        {
            if (await _context.Departments.AnyAsync(d => d.Name == dto.Name))
                return ApiResponse<DepartmentResponseDto>.Failure("A department with this name already exists.");

            var department = new Department
            {
                Name = dto.Name,
                DepartmentHeadId = string.IsNullOrWhiteSpace(dto.DepartmentHeadId) ? null : dto.DepartmentHeadId,
                IsActive = true
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return ApiResponse<DepartmentResponseDto>.Success(new DepartmentResponseDto { Id = department.Id, Name = department.Name });
        }

        public async Task<ApiResponse<DepartmentResponseDto>> UpdateAsync(UpdateDepartmentDto dto)
        {
            var dept = await _context.Departments.FindAsync(dto.Id);
            if (dept == null)
                return ApiResponse<DepartmentResponseDto>.Failure("Department not found.");

            // Prevent renaming the General department
            if (dept.Id == 1 && dto.Name != "General")
                return ApiResponse<DepartmentResponseDto>.Failure("The General department cannot be renamed.");

            dept.Name = dto.Name;
            dept.DepartmentHeadId = string.IsNullOrWhiteSpace(dto.DepartmentHeadId) ? null : dto.DepartmentHeadId;
            dept.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return ApiResponse<DepartmentResponseDto>.Success(new DepartmentResponseDto { Id = dept.Id, Name = dept.Name });
        }

        public async Task<ApiResponse<bool>> DeactivateAsync(int id)
        {
            // PRD RULE 1: The 'General' department must always exist as a fallback and cannot be deactivated
            if (id == 1)
                return ApiResponse<bool>.Failure("The General department cannot be deactivated.");

            var dept = await _context.Departments
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dept == null)
                return ApiResponse<bool>.Failure("Department not found.");

            // PRD RULE 2: A department can only be set as Inactive if no active user accounts are assigned to it
            if (dept.Users.Any(u => u.IsActive))
                return ApiResponse<bool>.Failure($"Cannot deactivate '{dept.Name}' because it currently has active users.");

            dept.IsActive = false;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Department deactivated successfully.");
        }
    }
}
