using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class RecurringTicket : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string TemplateName { get; set; } = string.Empty;

        // --- Ticket Blueprint (What the generated ticket looks like) ---
        [Required]
        [MaxLength(200)]
        public string TicketTitle { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Required]
        public TicketPriority Priority { get; set; }

        [MaxLength(450)]
        public string? AssignToUserId { get; set; } // Optional pre-assignment

        [ForeignKey("AssignToUserId")]
        public ApplicationUser? AssignToUser { get; set; }

        [Required]
        [MaxLength(450)]
        public string RaiseOnBehalfOfUserId { get; set; } = string.Empty;

        [ForeignKey("RaiseOnBehalfOfUserId")]
        public ApplicationUser? RaiseOnBehalfOfUser { get; set; }

        // --- Scheduling Rules ---
        [Required]
        [MaxLength(50)]
        public string RecurrencePattern { get; set; } = string.Empty; // "Daily", "Weekly", "Monthly", or CRON

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? MaxOccurrences { get; set; }

        public int CurrentRunCount { get; set; } = 0;

        public DateTime? NextRunDate { get; set; }

    }   // Note: The 'Status' (Active/Paused) required by the PRD is handled automatically by the 'IsActive' property inherited from BaseEntity!
}
