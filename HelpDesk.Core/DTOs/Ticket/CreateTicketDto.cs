using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Ticket
{
    public class CreateTicketDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public TicketPriority Priority { get; set; } 
        public string? RaisedForUserId { get; set; }
    }
}
