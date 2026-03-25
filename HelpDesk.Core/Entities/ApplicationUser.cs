using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }

        public ICollection<Ticket> RaisedTickets { get; set; } = new List<Ticket>();

        public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    }
}
