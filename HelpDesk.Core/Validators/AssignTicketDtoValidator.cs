using FluentValidation;
using HelpDesk.Core.DTOs.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Validators
{
    public class AssignTicketDtoValidator : AbstractValidator<AssignTicketDto>
    {
        public AssignTicketDtoValidator()
        {
            RuleFor(x => x.AgentId)
                .NotEmpty().WithMessage("Agent ID is required to assign a ticket.");

            // Add this rule to protect the hidden ID!
            RuleFor(x => x.TicketId)
                .GreaterThan(0).WithMessage("Valid Ticket ID is required.");
        }
    }
}
