using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Department
{
    public class DepartmentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public string? DepartmentHeadId { get; set; }
        public string? DepartmentHeadName { get; set; } // We send the name so Angular doesn't have to look it up!

        public int ActiveUserCount { get; set; } // PRD Requirement: Admin needs to see how many users are in it
    }
}
