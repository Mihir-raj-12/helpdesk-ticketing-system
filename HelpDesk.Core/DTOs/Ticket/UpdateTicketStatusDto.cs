using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Ticket
{
    public class UpdateTicketStatusDto
    {
        public int TicketId { get; set; } // <-- Add this!
        public string Status { get; set; } = string.Empty;
    }
}
