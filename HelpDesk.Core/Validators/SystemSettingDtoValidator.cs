using FluentValidation;
using HelpDesk.Core.DTOs.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Validators
{
    public class SystemSettingDtoValidator : AbstractValidator<SystemSettingDto>
    {
        public SystemSettingDtoValidator()
        {
            RuleFor(x => x.SystemName).NotEmpty();
            RuleFor(x => x.SupportEmailAddress).NotEmpty().EmailAddress();
            RuleFor(x => x.WorkingDays).NotEmpty();
            RuleFor(x => x.SlaCriticalResolutionHours).GreaterThan(0);
        }
    }
}
