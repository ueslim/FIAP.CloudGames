using FIAP.CloudGames.Core.Messages;
using FluentValidation;

namespace FIAP.CloudGames.Customer.API.Application.Commands
{
    public class RegisterCustomerCommand : Command
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Cpf { get; private set; }

        public RegisterCustomerCommand(Guid id, string name, string email, string cpf)
        {
            AggregateId = id;
            Id = id;
            Name = name;
            Email = email;
            Cpf = cpf;
        }

        public override bool IsValid()
        {
            ValidationResult = new RegisterCustomerValidation().Validate(this);
            return ValidationResult.IsValid;
        }

        public class RegisterCustomerValidation : AbstractValidator<RegisterCustomerCommand>
        {
            public RegisterCustomerValidation()
            {
                RuleFor(c => c.Id)
                    .NotEqual(Guid.Empty)
                    .WithMessage("Id do cliente inválido");

                RuleFor(c => c.Name)
                    .NotEmpty()
                    .WithMessage("O nome do cliente não foi informado");

                RuleFor(c => c.Cpf)
                    .Must(IsValidCpf)
                    .WithMessage("O CPF informado não é válido.");

                RuleFor(c => c.Email)
                    .Must(IsValidEmail)
                    .WithMessage("O e-mail informado não é válido.");
            }

            protected static bool IsValidCpf(string cpf)
            {
                return Core.DomainObjects.Cpf.IsValidCpf(cpf);
            }

            protected static bool IsValidEmail(string email)
            {
                return Core.DomainObjects.Email.IsValidEmail(email);
            }
        }
    }
}