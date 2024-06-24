using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.BusinessLogic.Implementation.Locations
{
    public class CreateLocationValidator : AbstractValidator<CreateLocationModel>
    {
        public CreateLocationValidator()
        {
            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("Please enter location name.")
                .MinimumLength(3).WithMessage("Location name should be at least 3 characters long.")
                .MaximumLength(50).WithMessage("Location name cannot be longer than 40 characters.");
            RuleFor(e => e.Latitude)
                .Must(l => l < 90 && l > -90).WithMessage("Please try again.");
            RuleFor(e => e.Longitude)
                .Must(l => l < 180 && l > -180).WithMessage("Choose a point on the map.");
            RuleFor(e => e.Address)
                .NotEmpty().WithMessage("There was an error with your location. Please try again later")
                .NotNull().WithMessage("There was an error with your location. Please try again later");

            RuleFor(e => e.LocationTypeId)
                .NotEqual(0).WithMessage("Please enter location's type");
        }
    }
}
