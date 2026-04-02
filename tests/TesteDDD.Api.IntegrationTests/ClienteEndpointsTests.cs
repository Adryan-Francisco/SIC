using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace TesteDDD.Api.IntegrationTests;

public class ClienteEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ClienteEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCliente_WithInvalidPayload_Returns400()
    {
        var payload = new
        {
            nome = "",
            endereco = "",
            cep = "abc"
        };

        var response = await _client.PostAsJsonAsync("/api/Cliente", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCliente_WithValidPayload_Returns201AndLocation()
    {
        var payload = new
        {
            nome = "Cliente Integracao",
            endereco = "Rua Teste, 100",
            cep = "12345-678"
        };

        var response = await _client.PostAsJsonAsync("/api/Cliente", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task GetCliente_ByUnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/Cliente/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCliente_WithShortNome_ReturnsBusinessCode()
    {
        var payload = new
        {
            nome = "ab",
            endereco = "Rua Teste, 100",
            cep = "12345-678"
        };

        var response = await _client.PostAsJsonAsync("/api/Cliente", payload);
        var responseBody = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(responseBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(json.RootElement.TryGetProperty("code", out var code));
        Assert.Equal("CLIENTE_NOME_CURTO", code.GetString());
    }
}
