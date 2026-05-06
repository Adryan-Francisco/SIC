using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestClienteJsonValidator : AbstractValidator<RequestClienteJson>
{
    public RequestClienteJsonValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome do cliente e obrigatorio.")
            .MaximumLength(100)
            .WithMessage("Nome do cliente nao pode ter mais de 100 caracteres.");

        RuleFor(x => x.Endereco)
            .NotEmpty()
            .WithMessage("Endereco do cliente e obrigatorio.")
            .MinimumLength(5)
            .WithMessage("Endereco deve ter pelo menos 5 caracteres.")
            .MaximumLength(200)
            .WithMessage("Endereco nao pode ter mais de 200 caracteres.");

        RuleFor(x => x.Cep)
            .NotEmpty()
            .WithMessage("CEP do cliente e obrigatorio.")
            .Matches(@"^(\d{8,9}|\d{5}-\d{3})$")
            .WithMessage("CEP invalido. Use 12345-678, 12345678 ou 123456789.");
    }
}
