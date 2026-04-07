using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Ticket
{
    public class UpdateTicketPriorityDto
    {
        public int TicketId { get; set; }
        public string Priority { get; set; } = string.Empty;
    }
}
