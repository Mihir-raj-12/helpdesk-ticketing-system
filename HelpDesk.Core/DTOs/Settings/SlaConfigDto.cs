using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Settings
{
    public class SlaConfigDto
    {
        public int Id { get; set; }

        [Required]
        public TicketPriority Priority { get; set; }

        [Required]
        [Range(1, 720)] // Max 1 month
        public int FirstResponseHours { get; set; }

        [Required]
        [Range(1, 720)]
        public int ResolutionHours { get; set; }

        [Required]
        [Range(1, 99)]
        public int WarningThresholdPercent { get; set; }
    }
}
