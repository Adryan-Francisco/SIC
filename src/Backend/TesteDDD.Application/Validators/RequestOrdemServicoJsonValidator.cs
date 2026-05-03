using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestOrdemServicoJsonValidator : AbstractValidator<RequestOrdemServicoJson>
{
    public RequestOrdemServicoJsonValidator()
    {
        RuleFor(x => x.ClienteId).NotEmpty();
        RuleFor(x => x.ProdutoId).NotEmpty();
        RuleFor(x => x.Descricao).NotEmpty().MinimumLength(5).MaximumLength(500);
        RuleFor(x => x.DataAbertura).NotEmpty();
        RuleFor(x => x.ValorServico).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status).InclusiveBetween(1, 4);
    }
}
