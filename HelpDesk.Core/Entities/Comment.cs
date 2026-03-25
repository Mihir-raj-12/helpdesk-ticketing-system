using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class Comment : BaseEntity
    {
        public string Content { get; set; } = string.Empty;

        // Foreign Keys
        public int TicketId { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        public Ticket Ticket { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
