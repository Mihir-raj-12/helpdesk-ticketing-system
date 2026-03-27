using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Comment
{
    public class CreateCommentDto
    {
        public string Content { get; set; } = string.Empty;
        public int TicketId { get; set; }
    }
}
