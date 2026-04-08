using FluentValidation;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Validators;

public class RequestProdutoJsonValidator : AbstractValidator<RequestProdutoJson>
{
    public RequestProdutoJsonValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome do produto é obrigatório.")
            .MinimumLength(3)
            .WithMessage("Nome do produto deve ter pelo menos 3 caracteres.")
            .MaximumLength(100)
            .WithMessage("Nome do produto não pode ter mais de 100 caracteres.");

        RuleFor(x => x.Preco)
            .GreaterThan(0)
            .WithMessage("Preço do produto deve ser maior que zero.");

        RuleFor(x => x.CategoriaId)
            .NotEmpty()
            .WithMessage("CategoriaId é obrigatório.");
    }
}
