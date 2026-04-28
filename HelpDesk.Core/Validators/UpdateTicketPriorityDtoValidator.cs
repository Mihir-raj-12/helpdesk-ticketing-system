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
    public class UpdateTicketPriorityDtoValidator : AbstractValidator<UpdateTicketPriorityDto>
    {
        public UpdateTicketPriorityDtoValidator()
        {
            RuleFor(x => x.Priority)
                   .IsInEnum()
                   .WithMessage("Invalid Ticket Priority. Please provide a valid priority ID.");

            RuleFor(x => x.TicketId)
                .GreaterThan(0).WithMessage("Ticket ID is required.");
        }
    }
}
