using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class Ticket : BaseEntity
    {
        public string Title { get; set; } = string .Empty;

        public string Dscription { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.Open;

        public TicketPriority Priority { get; set; }

        //Foreign key

        public int CategoryId { get; set; }

        public string RaisedByUserId { get; set; } = string.Empty;

        public string? AssignedTOUserId { get; set; }

        //Navigation Properties

        public Category Category { get; set; } = null!;

        public ApplicationUser RaisedByUser { get; set; } = null!;

        public ApplicationUser? AssignedToUser { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    }
}
