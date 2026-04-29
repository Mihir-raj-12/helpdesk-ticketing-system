using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Audit
{
    public class AuditLogResponseDto
    {
        public int Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string PerformedByUserId { get; set; } = string.Empty;
        public string PerformedByUserName { get; set; } = string.Empty; 
        public DateTime PerformedAt { get; set; }

        public List<AuditDetailDto> Details { get; set; } = new List<AuditDetailDto>();
    }
}
