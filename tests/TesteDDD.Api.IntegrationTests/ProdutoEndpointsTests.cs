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
            preco = 0,
            categoriaId = Guid.Empty,
            fornecedorId = Guid.Empty
        };

        var response = await _client.PostAsJsonAsync("/api/Produto", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduto_WithValidPayload_Returns201AndLocation()
    {
        var categoriaResponse = await _client.PostAsJsonAsync("/api/Categoria", new
        {
            name = "Categoria Produto",
            descricao = "Categoria valida para produto"
        });
        var categoria = await categoriaResponse.Content.ReadFromJsonAsync<JsonElement>();
        var categoriaId = categoria.GetProperty("id").GetGuid();

        var fornecedorResponse = await _client.PostAsJsonAsync("/api/Fornecedor", new
        {
            nome = "Fornecedor Produto",
            documento = "12345678000199",
            email = "fornecedor@teste.com",
            telefone = "11999999999"
        });
        var fornecedor = await fornecedorResponse.Content.ReadFromJsonAsync<JsonElement>();
        var fornecedorId = fornecedor.GetProperty("id").GetGuid();

        var payload = new
        {
            nome = "Produto Integracao",
            preco = 10.5m,
            categoriaId,
            fornecedorId
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
        var categoriaResponse = await _client.PostAsJsonAsync("/api/Categoria", new
        {
            name = "Categoria Teste",
            descricao = "Categoria valida para teste"
        });
        var categoria = await categoriaResponse.Content.ReadFromJsonAsync<JsonElement>();
        var categoriaId = categoria.GetProperty("id").GetGuid();

        var fornecedorResponse = await _client.PostAsJsonAsync("/api/Fornecedor", new
        {
            nome = "Fornecedor Teste",
            documento = "12345678000199",
            email = "fornecedor@teste.com",
            telefone = "11999999999"
        });
        var fornecedor = await fornecedorResponse.Content.ReadFromJsonAsync<JsonElement>();
        var fornecedorId = fornecedor.GetProperty("id").GetGuid();

        var payload = new
        {
            nome = "ab",
            preco = 10.5m,
            categoriaId,
            fornecedorId
        };

        var response = await _client.PostAsJsonAsync("/api/Produto", payload);
        var responseBody = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(responseBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(json.RootElement.TryGetProperty("code", out var code));
        Assert.Equal("PRODUTO_NOME_CURTO", code.GetString());
    }
}
