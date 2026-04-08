using Xunit;
using FluentValidation;
using TesteDDD.Application.Validators;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Tests;

public class RequestCategoriaJsonValidatorTests
{
    private readonly RequestCategoriaJsonValidator _validator;

    public RequestCategoriaJsonValidatorTests()
    {
        _validator = new RequestCategoriaJsonValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        // Arrange
        var request = new RequestCategoriaJson
        {
            Name = "Eletrônicos",
            Descricao = "Categoria de eletrônicos em geral"
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
        var request = new RequestCategoriaJson
        {
            Name = "",
            Descricao = "Categoria válida"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithShortDescription_ShouldFail()
    {
        // Arrange
        var request = new RequestCategoriaJson
        {
            Name = "Eletrônicos",
            Descricao = "abc"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Descricao");
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldFail()
    {
        // Arrange
        var request = new RequestCategoriaJson
        {
            Name = "Eletrônicos",
            Descricao = ""
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Descricao");
    }
}
