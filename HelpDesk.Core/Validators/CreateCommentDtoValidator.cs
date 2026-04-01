using FluentValidation;
using HelpDesk.Core.DTOs.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Validators
{
    public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.TicketId)
                .GreaterThan(0).WithMessage("Valid Ticket ID is required.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Comment content cannot be empty.")
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
        }
    }
}
