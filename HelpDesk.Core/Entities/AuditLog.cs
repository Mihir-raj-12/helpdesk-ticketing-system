using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class AuditLog : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)] // Using string because some PKs (like UserIds) are strings!
        public string EntityId { get; set; } = string.Empty;

        [MaxLength(45)] // 45 chars safely holds an IPv6 address
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? AdditionalNotes { get; set; }
        // -----------------------------

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(450)] // Standard ASP.NET Identity ID length
        public string? PerformedByUserId { get; set; } = string.Empty;

        public DateTime PerformedAt { get; set; }

        // Navigation Properties
        [ForeignKey("PerformedByUserId")]
        public ApplicationUser? PerformedByUser { get; set; } = null!;

        public ICollection<AuditDetail> AuditDetails { get; set; } = new List<AuditDetail>();

        [Required]
        public string ActorName { get; set; } = string.Empty;

        [Required]
        public string ActorEmail { get; set; } = string.Empty;

        [Required]
        public string ActorRole { get; set; } = string.Empty;
    }
}
