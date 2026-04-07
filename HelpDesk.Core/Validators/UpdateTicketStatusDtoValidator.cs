using FluentValidation;
using HelpDesk.Core.DTOs.Ticket;
using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Validators
{
    public class UpdateTicketStatusDtoValidator : AbstractValidator<UpdateTicketStatusDto>
    {
        public UpdateTicketStatusDtoValidator()
        {
            // The Fix: Use IsEnumName instead of IsInEnum
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .IsEnumName(typeof(TicketStatus), caseSensitive: false)
                .WithMessage("Invalid Ticket Status.");

            RuleFor(x => x.TicketId)
                .GreaterThan(0).WithMessage("Ticket ID is required.");
        }
    }
}
