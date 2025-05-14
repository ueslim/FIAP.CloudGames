using FIAP.CloudGames.Application.DTOs;
using FluentValidation;
using System.Text.RegularExpressions;

namespace FIAP.CloudGames.Application.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Must(BeAValidPassword).WithMessage("Password must contain at least one letter, one number, and one special character");
        }

        private bool BeAValidPassword(string password)
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasLetter = new Regex(@"[a-zA-Z]+");
            var hasSpecial = new Regex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+");

            return hasNumber.IsMatch(password) &&
                   hasLetter.IsMatch(password) &&
                   hasSpecial.IsMatch(password);
        }
    }
}