using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Dashboard
{
    public class DashboardResponseDto
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int OnHoldTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int LowPriorityTickets { get; set; }
        public int MediumPriorityTickets { get; set; }
        public int HighPriorityTickets { get; set; }
        public int CriticalPriorityTickets { get; set; }
        public int TicketsThisMonth { get; set; }
        public int TicketsLastMonth { get; set; }
        public List<TopAgentDto> TopAgents { get; set; } = new();
    }
}
