using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace TesteDDD.Api.IntegrationTests;

public class ProdutoEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProdutoEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduto_WithInvalidPayload_Returns400()
    {
        var payload = new
        {
            nome = "",
            preco = 0
        };

        var response = await _client.PostAsJsonAsync("/api/Produto", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduto_WithValidPayload_Returns201AndLocation()
    {
        var payload = new
        {
            nome = "Produto Integracao",
            preco = 10.5m
        };

        var response = await _client.PostAsJsonAsync("/api/Produto", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task GetProduto_ByUnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/Produto/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduto_WithShortNome_ReturnsBusinessCode()
    {
        var payload = new
        {
            nome = "ab",
            preco = 10.5m
        };

        var response = await _client.PostAsJsonAsync("/api/Produto", payload);
        var responseBody = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(responseBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(json.RootElement.TryGetProperty("code", out var code));
        Assert.Equal("PRODUTO_NOME_CURTO", code.GetString());
    }
}
