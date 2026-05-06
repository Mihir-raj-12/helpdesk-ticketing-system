using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class SystemSetting : BaseEntity
    {

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

        public string? CompanyLogoUrl { get; set; }

        public string DefaultTimeZone { get; set; } = "UTC";

        [Range(15, 480)] // PRD: Configurable between 15 mins and 8 hours (480 mins)
        public int SessionTimeoutMinutes { get; set; } = 60;

    }
}
