using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Ticket
{
    public class SubmitFeedbackDto
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "CSAT score must be between 1 and 5.")]
        public int CsatScore { get; set; }

        [MaxLength(500, ErrorMessage = "Comments cannot exceed 500 characters.")]
        public string? Comments { get; set; }
    }
}
