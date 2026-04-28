using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Ticket
{
    public class UpdateTicketStatusDto
    {
        public int TicketId { get; set; }
        public TicketStatus Status { get; set; }
    }
}
