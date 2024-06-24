using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.CommonFunc;

namespace TicketApp.BusinessLogic.Implementation.Artists
{
    public class CreateArtistValidator : AbstractValidator<CreateArtistModel>
    {
        public CreateArtistValidator()
        {
            RuleFor(t => t.Name)
            .NotEmpty().WithMessage("Please enter artist name.")
            .MinimumLength(3).WithMessage("Artist name should be at least 3 characters long.")
            .MaximumLength(100).WithMessage("Artist name cannot be longer than 100 characters.");

            RuleFor(r => r.Picture)
                .NotNull().WithMessage("Please enter a picture")
                .NotEmpty().WithMessage("Please enter a picture")
                .Must(ValidationFunctions.CorrectFileExtension).WithMessage("Add png or jpeg file");

            RuleFor(r => r.Birthdate)
                .NotEmpty().WithMessage("Please enter artist birth date")
                .Must(ValidationFunctions.BeInPast).WithMessage("Birth date should be in the past");

            RuleFor(r => r.ArtistTypeId)
                .NotEqual(0).WithMessage("Please enter artist type");

            RuleFor(r => r.Debut)
                .NotEmpty().WithMessage("Please enter artist's debut date")
                .Must(ValidationFunctions.BeInPast).WithMessage("Debut date should be in the past");

            RuleFor(r => r)
                .Must(d => d.Debut > d.Birthdate).WithMessage("Debut date cannot occur before birth date");
        }
    }
}
