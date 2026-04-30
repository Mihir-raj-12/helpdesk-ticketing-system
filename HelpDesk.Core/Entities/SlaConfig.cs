using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class SlaConfig : BaseEntity
    {

        [Required]
        public TicketPriority Priority { get; set; }

        [Required]
        public int FirstResponseHours { get; set; }

        [Required]
        public int ResolutionHours { get; set; }

        [Required]
        public int WarningThresholdPercent { get; set; } = 75; // PRD explicitly demands 75%
    }
}
