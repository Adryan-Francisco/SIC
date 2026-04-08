using Xunit;
using FluentValidation;
using TesteDDD.Application.Validators;
using TesteDDD.Communication.Requests;

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
        // Arrange
        var request = new RequestProdutoJson
        {
            Nome = "Produto Teste",
            Preco = 100m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        // Arrange
        var request = new RequestProdutoJson
        {
            Nome = "",
            Preco = 100m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_WithShortName_ShouldFail()
    {
        // Arrange
        var request = new RequestProdutoJson
        {
            Nome = "AB",
            Preco = 100m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_WithZeroPrice_ShouldFail()
    {
        // Arrange
        var request = new RequestProdutoJson
        {
            Nome = "Produto Teste",
            Preco = 0,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Preco");
    }

    [Fact]
    public void Validate_WithEmptyCategoriaId_ShouldFail()
    {
        // Arrange
        var request = new RequestProdutoJson
        {
            Nome = "Produto Teste",
            Preco = 100m,
            CategoriaId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CategoriaId");
    }
}
