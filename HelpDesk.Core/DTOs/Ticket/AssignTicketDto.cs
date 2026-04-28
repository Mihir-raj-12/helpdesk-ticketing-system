using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Ticket
{
    public class AssignTicketDto
    {
        public int TicketId { get; set; } 
        public string AgentId { get; set; } = string.Empty;
    }
}
