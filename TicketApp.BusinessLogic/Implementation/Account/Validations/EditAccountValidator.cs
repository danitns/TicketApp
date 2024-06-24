using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.CommonFunc;
using TicketApp.DataAccess;

namespace TicketApp.BusinessLogic.Implementation.Account
{
    public class EditAccountValidator : AbstractValidator<EditUserModel>
    {
        private readonly UnitOfWork _unitOfWork;
        public EditAccountValidator(UnitOfWork unitOfWork)
        {

            RuleFor(r => r.FirstName)
                .NotEmpty().WithMessage("Please enter yout first name.")
                .MinimumLength(3).WithMessage("Your first name should be at least 3 characters long.")
                .MaximumLength(40).WithMessage("Your first name cannot be longer than 40 characters.")
                .Must(ValidationFunctions.ContainsOnlyLetters).WithMessage("Please enter only letters.");

            RuleFor(r => r.LastName)
                .NotEmpty().WithMessage("Please enter your last name.")
                .MinimumLength(3).WithMessage("Your last name should be at least 3 characters long.")
                .MaximumLength(40).WithMessage("Your last name cannot be longer than 40 characters.")
                .Must(ValidationFunctions.ContainsOnlyLetters).WithMessage("Please enter only letters."); 

            RuleFor(r => r.Phone)
                .Length(10, 12).WithMessage("Phone number length should be between 10 and 12 characters")
                .NotEmpty().WithMessage("Please enter your phone number")
                .Must(NotAlreadyExistPhone).WithMessage("Phone number already exists")
                .Must(ValidationFunctions.ContainsOnlyNumbersWithOptionalPlus).WithMessage("Enter valid a format");

            RuleFor(r => r.NewPicture)
                .Must(ValidationFunctions.CorrectFileExtension).WithMessage("Add png or jpeg file");

            _unitOfWork = unitOfWork;
        }

        public bool NotAlreadyExistPhone(EditUserModel model, string phone)
        {
            var user = _unitOfWork.Users
                .Get()
                .SingleOrDefault(u => u.Email == model.Email);
            var usersWithTheSamePhone = !_unitOfWork.Users.Get().Any(u => u.Phone == phone && user.Phone != phone);
            return usersWithTheSamePhone;
        }
    }
}
