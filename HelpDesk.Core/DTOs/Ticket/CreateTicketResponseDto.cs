using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Ticket
{
    public class CreateTicketResponseDto
    {
        public int Id { get; set; }
        public TicketStatus Status { get; set; }
    }
}
