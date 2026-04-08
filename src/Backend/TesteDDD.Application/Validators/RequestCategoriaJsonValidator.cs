using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestCategoriaJsonValidator : AbstractValidator<RequestCategoriaJson>
{
    public RequestCategoriaJsonValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome da categoria é obrigatório.")
            .MinimumLength(3)
            .WithMessage("Nome da categoria deve ter pelo menos 3 caracteres.")
            .MaximumLength(50)
            .WithMessage("Nome da categoria não pode ter mais de 50 caracteres.");

        RuleFor(x => x.Descricao)
            .NotEmpty()
            .WithMessage("Descrição da categoria é obrigatória.")
            .MinimumLength(5)
            .WithMessage("Descrição deve ter pelo menos 5 caracteres.")
            .MaximumLength(200)
            .WithMessage("Descrição não pode ter mais de 200 caracteres.");
    }
}
