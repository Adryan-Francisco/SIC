using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestVendasJsonValidator : AbstractValidator<RequestVendasJson>
{
    public RequestVendasJsonValidator()
    {
        RuleFor(x => x.DataVenda)
            .NotEmpty()
            .WithMessage("Data da venda e obrigatoria.")
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Data da venda nao pode ser no futuro.");

        RuleFor(x => x.Itens)
            .NotEmpty()
            .WithMessage("A venda deve conter pelo menos um item.");
    }
}
