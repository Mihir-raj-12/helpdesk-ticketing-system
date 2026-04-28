using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class SystemSetting
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string SystemName { get; set; } = "Help Desk Pro";

        [Required]
        [MaxLength(100)]
        public string SupportEmailAddress { get; set; } = "support@company.com";

        // Business Hours
        public TimeSpan BusinessHourStart { get; set; } = new TimeSpan(9, 0, 0); // 9:00 AM
        public TimeSpan BusinessHourEnd { get; set; } = new TimeSpan(17, 0, 0); // 5:00 PM

        [Required]
        public string WorkingDays { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";

        // SLA Resolution Targets (in Hours)
        public int SlaCriticalResolutionHours { get; set; } = 4;
        public int SlaHighResolutionHours { get; set; } = 8;
        public int SlaMediumResolutionHours { get; set; } = 24; // 3 business days (assuming 8hr days)
        public int SlaLowResolutionHours { get; set; } = 40;  // 5 business days
    }
}
