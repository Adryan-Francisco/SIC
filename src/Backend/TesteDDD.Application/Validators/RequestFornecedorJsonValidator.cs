using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestFornecedorJsonValidator : AbstractValidator<RequestFornecedorJson>
{
    public RequestFornecedorJsonValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(150);

        RuleFor(x => x.Documento)
            .NotEmpty()
            .MinimumLength(11)
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .MaximumLength(150);

        RuleFor(x => x.Telefone)
            .MaximumLength(20);
    }
}
