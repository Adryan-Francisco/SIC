using Xunit;
using FluentValidation;
using TesteDDD.Application.Validators;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Application.Tests;

public class RequestClienteJsonValidatorTests
{
    private readonly RequestClienteJsonValidator _validator;

    public RequestClienteJsonValidatorTests()
    {
        _validator = new RequestClienteJsonValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        // Arrange
        var request = new RequestClienteJson
        {
            Nome = "João Silva",
            Endereco = "Rua das Flores, 123",
            Cep = "12345678"
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
        var request = new RequestClienteJson
        {
            Nome = "",
            Endereco = "Rua das Flores, 123",
            Cep = "12345678"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_WithInvalidCep_ShouldFail()
    {
        // Arrange
        var request = new RequestClienteJson
        {
            Nome = "João Silva",
            Endereco = "Rua das Flores, 123",
            Cep = "ABC"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Cep");
    }

    [Fact]
    public void Validate_WithEmptyAddress_ShouldFail()
    {
        // Arrange
        var request = new RequestClienteJson
        {
            Nome = "João Silva",
            Endereco = "",
            Cep = "12345678"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Endereco");
    }

    [Fact]
    public void Validate_WithValidCepNineDigits_ShouldSucceed()
    {
        // Arrange
        var request = new RequestClienteJson
        {
            Nome = "João Silva",
            Endereco = "Rua das Flores, 123",
            Cep = "123456789"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }
}
