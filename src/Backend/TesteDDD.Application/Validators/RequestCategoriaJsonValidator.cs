using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestCategoriaJsonValidator : AbstractValidator<RequestCategoriaJson>
{
    public RequestCategoriaJsonValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome da categoria e obrigatorio.")
            .MaximumLength(50)
            .WithMessage("Nome da categoria nao pode ter mais de 50 caracteres.");

        RuleFor(x => x.Descricao)
            .NotEmpty()
            .WithMessage("Descricao da categoria e obrigatoria.")
            .MinimumLength(5)
            .WithMessage("Descricao deve ter pelo menos 5 caracteres.")
            .MaximumLength(200)
            .WithMessage("Descricao nao pode ter mais de 200 caracteres.");
    }
}
