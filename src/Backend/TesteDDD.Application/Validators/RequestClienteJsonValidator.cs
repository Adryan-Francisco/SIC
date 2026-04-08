using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestClienteJsonValidator : AbstractValidator<RequestClienteJson>
{
    public RequestClienteJsonValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome do cliente é obrigatório.")
            .MinimumLength(3)
            .WithMessage("Nome do cliente deve ter pelo menos 3 caracteres.")
            .MaximumLength(100)
            .WithMessage("Nome do cliente não pode ter mais de 100 caracteres.");

        RuleFor(x => x.Endereco)
            .NotEmpty()
            .WithMessage("Endereço do cliente é obrigatório.")
            .MinimumLength(5)
            .WithMessage("Endereço deve ter pelo menos 5 caracteres.")
            .MaximumLength(200)
            .WithMessage("Endereço não pode ter mais de 200 caracteres.");

        RuleFor(x => x.Cep)
            .NotEmpty()
            .WithMessage("CEP do cliente é obrigatório.")
            .Matches(@"^\d{8,9}$")
            .WithMessage("CEP deve conter apenas dígitos (8 ou 9 dígitos).");
    }
}
