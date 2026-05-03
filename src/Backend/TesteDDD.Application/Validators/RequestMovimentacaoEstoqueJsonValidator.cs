using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestMovimentacaoEstoqueJsonValidator : AbstractValidator<RequestMovimentacaoEstoqueJson>
{
    public RequestMovimentacaoEstoqueJsonValidator()
    {
        RuleFor(x => x.ProdutoId).NotEmpty();
        RuleFor(x => x.Quantidade).GreaterThan(0);
        RuleFor(x => x.Observacao).MaximumLength(250);
    }
}
