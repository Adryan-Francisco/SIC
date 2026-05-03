using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestProdutoJsonValidator : AbstractValidator<RequestProdutoJson>
{
    public RequestProdutoJsonValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome do produto e obrigatorio.")
            .MinimumLength(3)
            .WithMessage("Nome do produto deve ter pelo menos 3 caracteres.")
            .MaximumLength(100)
            .WithMessage("Nome do produto nao pode ter mais de 100 caracteres.");

        RuleFor(x => x.Preco)
            .GreaterThan(0)
            .WithMessage("Preco do produto deve ser maior que zero.");

        RuleFor(x => x.CategoriaId)
            .NotEmpty()
            .WithMessage("CategoriaId e obrigatorio.");

        RuleFor(x => x.FornecedorId)
            .NotEmpty()
            .WithMessage("FornecedorId e obrigatorio.");
    }
}
