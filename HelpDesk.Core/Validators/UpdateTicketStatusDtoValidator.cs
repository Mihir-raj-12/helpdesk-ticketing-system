using FluentValidation;
using HelpDesk.Core.DTOs.Ticket;
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
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid Ticket Status.");
        }
    }
}
