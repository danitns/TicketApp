﻿using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.CommonFunc;
using TicketApp.DataAccess;

namespace TicketApp.BusinessLogic.Implementation.Events
{
    public class CreateEventValidator : AbstractValidator<CreateEventModel>
    {

        public CreateEventValidator()
        {
            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("Please enter event name.")
                .MinimumLength(3).WithMessage("Event name should be at least 3 characters long.")
                .MaximumLength(200).WithMessage("Event name cannot be longer than 200 characters.");

            RuleFor(r => r.Picture)
                .NotNull().WithMessage("Please enter a picture")
                .NotEmpty().WithMessage("Please enter a picture")
                .Must(ValidationFunctions.CorrectFileExtension).WithMessage("Add png or jpeg file");

            RuleFor(r => r.Description)
                .NotEmpty().WithMessage("Please enter a description")
                .MinimumLength(10).WithMessage("Event description should be at least 10 characters long.")
                .MaximumLength(1000).WithMessage("Event description cannot be longer than 200 characters.");

            RuleFor(r => r.StartDate)
                .NotEmpty().WithMessage("Please enter a start date")
                .Must(ValidationFunctions.BeInFuture).WithMessage("You cannot create an event in the past");

            RuleFor(r => r.EndDate)
                .NotEmpty().WithMessage("Please enter a start date")
                .Must(ValidationFunctions.BeInFuture).WithMessage("You cannot create an event in the past");

            RuleFor(r => r)
                .Must(d => d.StartDate < d.EndDate).WithMessage("End date cannot take place before start date");

        }

        
    }
}
