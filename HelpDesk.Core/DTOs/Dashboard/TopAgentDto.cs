using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Dashboard
{
    public class TopAgentDto
    {

        public string AgentName { get; set; } = string.Empty;
        public int ResolvedTicketsCount { get; set; }
    }
}
