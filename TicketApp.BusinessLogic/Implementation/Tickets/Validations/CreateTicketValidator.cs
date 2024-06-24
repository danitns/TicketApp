using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.CommonFunc;

namespace TicketApp.BusinessLogic.Implementation.Tickets
{
    public class CreateTicketValidator : AbstractValidator<CreateTicketModel>
    {
        public CreateTicketValidator()
        {
            RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Please enter ticket name.")
                .MinimumLength(3).WithMessage("Ticket name should be at least 3 characters long.")
                .MaximumLength(100).WithMessage("Ticket name cannot be longer than 100 characters.")
                .Must(ValidationFunctions.ContainsOnlyLettersAndNumbers).WithMessage("Please enter only letters and numbers.");

            RuleFor(t => t.Price)
                .NotEmpty().WithMessage("Please enter ticket price.")
                .GreaterThanOrEqualTo(0).WithMessage("You cannot enter a negative price")
                .LessThan(1_000_000).WithMessage("The price is too large. Enter a value less than 1 000 000");

            RuleFor(t => t.Description)
               .NotEmpty().WithMessage("Please enter ticket description.")
               .MinimumLength(10).WithMessage("Ticket description should be at least 3 characters long.")
               .MaximumLength(1000).WithMessage("Ticket description cannot be longer than 100 characters.");

            RuleFor(r => r.Notickets)
                .NotEmpty().WithMessage("Please enter event's number of tickets")
                .GreaterThan(0).WithMessage("Please enter a value greater than 0")
                .LessThan(100_000_000).WithMessage("Please enter a value less than 100 000 000");

        }
    }
}
