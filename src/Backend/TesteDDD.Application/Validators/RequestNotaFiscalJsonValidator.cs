using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestEmitirNotaFiscalJsonValidator : AbstractValidator<RequestEmitirNotaFiscalJson>
{
    public RequestEmitirNotaFiscalJsonValidator()
    {
        RuleFor(x => x.VendasId)
            .NotEmpty()
            .WithMessage("VendasId é obrigatório.");

        RuleFor(x => x.Serie)
            .GreaterThan(0)
            .WithMessage("Série deve ser maior que zero.");

        RuleFor(x => x.Numero)
            .GreaterThan(0)
            .WithMessage("Número deve ser maior que zero.");
    }
}

public class RequestCancelarNotaFiscalJsonValidator : AbstractValidator<RequestCancelarNotaFiscalJson>
{
    public RequestCancelarNotaFiscalJsonValidator()
    {
        RuleFor(x => x.Justificativa)
            .NotEmpty()
            .WithMessage("Justificativa é obrigatória.");

        RuleFor(x => x.Justificativa)
            .MinimumLength(15)
            .WithMessage("Justificativa deve ter no mínimo 15 caracteres.");

        RuleFor(x => x.Justificativa)
            .MaximumLength(255)
            .WithMessage("Justificativa não pode ter mais de 255 caracteres.");
    }
}
