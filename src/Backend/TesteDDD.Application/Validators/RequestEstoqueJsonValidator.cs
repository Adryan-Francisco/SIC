using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestEstoqueJsonValidator : AbstractValidator<RequestEstoqueJson>
{
    public RequestEstoqueJsonValidator()
    {
        RuleFor(x => x.ProdutoId).NotEmpty();
        RuleFor(x => x.QuantidadeDisponivel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QuantidadeMinima).GreaterThanOrEqualTo(0);
    }
}
