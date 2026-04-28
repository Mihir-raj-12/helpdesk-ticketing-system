using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Settings
{
    public class SystemSettingDto
    {
        [Required]
        public string SystemName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string SupportEmailAddress { get; set; } = string.Empty;

        public TimeSpan BusinessHourStart { get; set; }
        public TimeSpan BusinessHourEnd { get; set; }

        [Required]
        public string WorkingDays { get; set; } = string.Empty;

        public int SlaCriticalResolutionHours { get; set; }
        public int SlaHighResolutionHours { get; set; }
        public int SlaMediumResolutionHours { get; set; }
        public int SlaLowResolutionHours { get; set; }
    }
}
