using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace TesteDDD.Api.IntegrationTests;

public class CategoriaEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriaEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCategoria_WithInvalidPayload_Returns400()
    {
        var payload = new
        {
            name = "",
            descricao = ""
        };

        var response = await _client.PostAsJsonAsync("/api/Categoria", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategoria_WithValidPayload_Returns201AndLocation()
    {
        var payload = new
        {
            name = "Categoria Integracao",
            descricao = "Descricao valida"
        };

        var response = await _client.PostAsJsonAsync("/api/Categoria", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task GetCategoria_ByUnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/Categoria/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategoria_WithShortNome_ReturnsBusinessCode()
    {
        var payload = new
        {
            name = "ab",
            descricao = "Descricao valida"
        };

        var response = await _client.PostAsJsonAsync("/api/Categoria", payload);
        var responseBody = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(responseBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(json.RootElement.TryGetProperty("code", out var code));
        Assert.Equal("CATEGORIA_NOME_CURTO", code.GetString());
    }
}
