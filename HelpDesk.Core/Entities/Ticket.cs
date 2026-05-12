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
    public class Ticket : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string .Empty;


        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.Open;

        public TicketPriority Priority { get; set; }

        //Foreign key

        public int CategoryId { get; set; }

        public string RaisedByUserId { get; set; } = string.Empty;

        public string? AssignedToUserId { get; set; }

        //Navigation Properties

        public Category Category { get; set; } = null!;

        public ApplicationUser RaisedByUser { get; set; } = null!;

        public ApplicationUser? AssignedToUser { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Add these inside your Ticket class
        public DateTime SlaDeadline { get; set; }

        // PRD Section 3 & 10: Marks whether the ticket has been formally escalated
        public bool EscalationFlag { get; set; } = false;

        // PRD Section 10: Escalation Tracking
        public string? EscalationReason { get; set; }
        public DateTime? EscalatedAt { get; set; }
        public DateTime? EscalationAcknowledgedAt { get; set; }

        [Required]
        public int DepartmentId { get; set; } // The department of the user who raised it

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public string? AffectedAsset { get; set; } // e.g., "Laptop SN-2341"

        public int? RelatedTicketId { get; set; } // Link to another ticket

        [ForeignKey("RelatedTicketId")]
        public Ticket? RelatedTicket { get; set; }

        public bool? SlaClosedWithinSla { get; set; } // Locked in when closed

        public DateTime? ArchivedAt { get; set; } // For the archival background worker

        public DateTime? SlaPausedAt { get; set; } // Tracks when the ticket was put On Hold
    }
}
