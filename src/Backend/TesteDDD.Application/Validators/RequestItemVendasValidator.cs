using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestItemVendasValidator : AbstractValidator<RequestItemVendasJson>
{
    public RequestItemVendasValidator()
    {
        RuleFor(x => x.IdProduto)
            .NotEmpty()
            .WithMessage("IdProduto e obrigatorio.");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero.");

        RuleFor(x => x.PrecoUnitario)
            .GreaterThan(0)
            .WithMessage("Preco unitario deve ser maior que zero.");
    }
}
