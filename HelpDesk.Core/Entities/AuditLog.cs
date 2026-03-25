using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class AuditLog
    {
        public string TableName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string PerformedByUserId { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }

        // Navigation Properties
        public ApplicationUser PerformedByUser { get; set; } = null!;
        public ICollection<AuditDetail> AuditDetails { get; set; }
            = new List<AuditDetail>();
    }
}
