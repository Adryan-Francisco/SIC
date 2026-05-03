using FluentValidation;
using TesteDDD.Application.Validators;
using TesteDDD.Communication.Requests;
using Xunit;

namespace TesteDDD.Application.Tests;

public class RequestProdutoJsonValidatorTests
{
    private readonly RequestProdutoJsonValidator _validator;

    public RequestProdutoJsonValidatorTests()
    {
        _validator = new RequestProdutoJsonValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var request = new RequestProdutoJson
        {
            Nome = "Produto Teste",
            Preco = 100m,
            CategoriaId = Guid.NewGuid(),
            FornecedorId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var request = new RequestProdutoJson
        {
            Nome = "",
            Preco = 100m,
            CategoriaId = Guid.NewGuid(),
            FornecedorId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_WithShortName_ShouldFail()
    {
        var request = new RequestProdutoJson
        {
            Nome = "AB",
            Preco = 100m,
            CategoriaId = Guid.NewGuid(),
            FornecedorId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_WithZeroPrice_ShouldFail()
    {
        var request = new RequestProdutoJson
        {
            Nome = "Produto Teste",
            Preco = 0,
            CategoriaId = Guid.NewGuid(),
            FornecedorId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Preco");
    }

    [Fact]
    public void Validate_WithEmptyCategoriaId_ShouldFail()
    {
        var request = new RequestProdutoJson
        {
            Nome = "Produto Teste",
            Preco = 100m,
            CategoriaId = Guid.Empty,
            FornecedorId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CategoriaId");
    }

    [Fact]
    public void Validate_WithEmptyFornecedorId_ShouldFail()
    {
        var request = new RequestProdutoJson
        {
            Nome = "Produto Teste",
            Preco = 100m,
            CategoriaId = Guid.NewGuid(),
            FornecedorId = Guid.Empty
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FornecedorId");
    }
}
