using System;
using System.Collections.Generic;
using System.Linq;
namespace HelpDesk.Core.Entities
{
    public class AuditDetail : BaseEntity
    {
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        // Foreign Key
        public int AuditLogId { get; set; }

        // Navigation Property
        public AuditLog AuditLog { get; set; } = null!;
    }
}

