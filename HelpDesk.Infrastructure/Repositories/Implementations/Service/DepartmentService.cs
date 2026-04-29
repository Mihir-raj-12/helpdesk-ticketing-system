using AutoMapper;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<DepartmentResponseDto>>> GetAllAsync()
        {
            // Cleanly fetches using our new custom repository method!
            var departments = await _unitOfWork.Departments.GetAllWithDetailsAsync();
            var responseDto = _mapper.Map<IEnumerable<DepartmentResponseDto>>(departments);
            return ApiResponse<IEnumerable<DepartmentResponseDto>>.Success(responseDto);
        }

        public async Task<ApiResponse<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto)
        {
            var department = _mapper.Map<Department>(dto);

            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            var responseDto = _mapper.Map<DepartmentResponseDto>(department);
            return ApiResponse<DepartmentResponseDto>.Success(responseDto, "Department created successfully.");
        }

        public async Task<ApiResponse<DepartmentResponseDto>> UpdateAsync(UpdateDepartmentDto dto)
        {
            var dept = await _unitOfWork.Departments.GetByIdAsync(dto.Id);
            if (dept == null)
                return ApiResponse<DepartmentResponseDto>.Failure("Department not found.");

            // PRD Rule: Prevent renaming the General department
            if (dept.Id == 1 && dto.Name != "General")
                return ApiResponse<DepartmentResponseDto>.Failure("The General department cannot be renamed.");

            dept.Name = dto.Name;
            dept.DepartmentHeadId = string.IsNullOrWhiteSpace(dto.DepartmentHeadId) ? null : dto.DepartmentHeadId;
            dept.IsActive = dto.IsActive;

            // Using your GenericRepository UpdateAsync!
            await _unitOfWork.Departments.UpdateAsync(dept, d => d.Name, d => d.DepartmentHeadId, d => d.IsActive);
            await _unitOfWork.SaveChangesAsync();

            var responseDto = _mapper.Map<DepartmentResponseDto>(dept);
            return ApiResponse<DepartmentResponseDto>.Success(responseDto, "Department updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeactivateAsync(int id)
        {
            if (id == 1)
                return ApiResponse<bool>.Failure("The General department cannot be deactivated.");

            // Use the specific repository method to get the Users count
            var dept = await _unitOfWork.Departments.GetByIdWithUsersAsync(id);

            if (dept == null) return ApiResponse<bool>.Failure("Department not found.");

            if (dept.Users.Any(u => u.IsActive))
                return ApiResponse<bool>.Failure($"Cannot deactivate '{dept.Name}' because it has active users.");

            // Using your existing SoftDeleteAsync from GenericRepository!
            await _unitOfWork.Departments.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Department deactivated successfully.");
        }



    }
}
