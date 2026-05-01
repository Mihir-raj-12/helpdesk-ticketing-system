using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Ticket
{
    public class EscalateTicketDto
    {
        // PRD: Mandatory reason, max 500 characters
        [Required(ErrorMessage = "An escalation reason is strictly required.")]
        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}
