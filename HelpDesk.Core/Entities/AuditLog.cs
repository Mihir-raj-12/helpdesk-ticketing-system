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
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(450)] // Standard ASP.NET Identity ID length
        public string PerformedByUserId { get; set; } = string.Empty;

        public DateTime PerformedAt { get; set; }

        // Navigation Properties
        [ForeignKey("PerformedByUserId")]
        public ApplicationUser PerformedByUser { get; set; } = null!;

        public ICollection<AuditDetail> AuditDetails { get; set; } = new List<AuditDetail>();
    }
}
