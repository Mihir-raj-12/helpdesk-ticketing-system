using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Department;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IDepartmentService
    {
        Task<ApiResponse<IEnumerable<DepartmentResponseDto>>> GetAllAsync();
        Task<ApiResponse<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto);
        Task<ApiResponse<DepartmentResponseDto>> UpdateAsync(UpdateDepartmentDto dto);
        Task<ApiResponse<bool>> DeactivateAsync(int id);
    }
}
