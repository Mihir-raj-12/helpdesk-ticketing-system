using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace HelpDesk.Core.Entities
{
    public class AuditDetail : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FieldName { get; set; } = string.Empty;

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        // Foreign Key
        [Required]
        public int AuditLogId { get; set; }

        // Navigation Property
        [ForeignKey("AuditLogId")]
        public AuditLog AuditLog { get; set; } = null!;
    }
}

