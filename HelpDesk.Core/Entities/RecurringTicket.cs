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
        // --- 1. The Blueprint Data (What the ticket looks like) ---
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Required]
        [MaxLength(450)]
        public string RaisedByUserId { get; set; } = string.Empty;

        // --- 2. The Scheduling Data (When it should run) ---

        [Required]
        [MaxLength(50)]
        public string Frequency { get; set; } = string.Empty; // e.g., "Daily", "Weekly", "Monthly"

        public DateTime NextRunDate { get; set; }
    }
}
