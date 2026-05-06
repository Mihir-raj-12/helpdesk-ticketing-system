using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class NotificationPreference : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        // The toggles
        public bool NotifyOnTicketCreated { get; set; } = true;
        public bool NotifyOnTicketAssigned { get; set; } = true;
        public bool NotifyOnStatusChanged { get; set; } = true;
        public bool NotifyOnNewComment { get; set; } = true;
        public bool NotifyOnTicketClosed { get; set; } = true;
        public bool OptOutCsatSurveys { get; set; } = false;

        // Note: The PRD states SLA breaches and Escalation alerts are MANDATORY for Admins. 
        // We will enforce that logic in the Service layer, not the database.
    }
}
