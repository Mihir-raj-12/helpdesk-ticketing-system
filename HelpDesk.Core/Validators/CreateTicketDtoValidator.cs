using FluentValidation;
using HelpDesk.Core.DTOs.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Validators
{
    public class CreateTicketDtoValidator : AbstractValidator<CreateTicketDto>
    { 
        public CreateTicketDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(150).WithMessage("Title cannot exceed 150 characters.");
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("CategoryId must be a positive integer.");
            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Priority must be a valid enum value.");
        }
    }
}
