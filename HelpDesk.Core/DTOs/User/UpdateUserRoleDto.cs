using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.User
{
    public class UpdateUserRoleDto
    {
        public string UserId { get; set; } = string.Empty;
        public UserRole NewRole { get; set; }
    }
}
