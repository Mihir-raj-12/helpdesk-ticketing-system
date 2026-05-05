using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class TicketFeedback : BaseEntity
    {
        [Required]
        public int TicketId { get; set; }

        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; } = null!;

        [Required]
        [Range(1, 5)]
        public int CsatScore { get; set; } // 1 = Very Unsatisfied, 5 = Very Satisfied

        [MaxLength(500)]
        public string? Comments { get; set; }

        [Required]
        public string SubmittedByUserId { get; set; } = string.Empty;

        [ForeignKey("SubmittedByUserId")]
        public ApplicationUser SubmittedByUser { get; set; } = null!;
    }
}
