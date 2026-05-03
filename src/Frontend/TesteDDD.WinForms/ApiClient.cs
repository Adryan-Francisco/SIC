using System.Net.Http.Json;
using System.Text.Json;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;

namespace TesteDDD.WinForms;

internal sealed class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(string baseAddress)
    {
        _httpClient = new HttpClient();
        SetBaseAddress(baseAddress);
    }

    public void SetBaseAddress(string baseAddress)
    {
        if (!baseAddress.EndsWith('/'))
        {
            baseAddress += "/";
        }

        _httpClient.BaseAddress = new Uri(baseAddress);
    }

    public async Task<List<ResponseFornecedorJson>> GetFornecedoresAsync()
        => await GetAsync<List<ResponseFornecedorJson>>("api/Fornecedor") ?? [];

    public async Task<ResponseFornecedorJson> SaveFornecedorAsync(Guid? id, RequestFornecedorJson request)
        => id.HasValue
            ? await PutAsync<ResponseFornecedorJson>($"api/Fornecedor/{id}", request)
            : await PostAsync<ResponseFornecedorJson>("api/Fornecedor", request);

    public Task DeleteFornecedorAsync(Guid id) => DeleteAsync($"api/Fornecedor/{id}");

    public async Task<List<ResponseProdutoJson>> GetProdutosAsync()
        => await GetAsync<List<ResponseProdutoJson>>("api/Produto") ?? [];

    public async Task<ResponseProdutoJson> SaveProdutoAsync(Guid? id, RequestProdutoJson request)
        => id.HasValue
            ? await PutAsync<ResponseProdutoJson>($"api/Produto/{id}", request)
            : await PostAsync<ResponseProdutoJson>("api/Produto", request);

    public Task DeleteProdutoAsync(Guid id) => DeleteAsync($"api/Produto/{id}");

    public async Task<List<ResponseCategoriaJson>> GetCategoriasAsync()
        => await GetAsync<List<ResponseCategoriaJson>>("api/Categoria") ?? [];

    public async Task<List<ResponseClienteJson>> GetClientesAsync()
        => await GetAsync<List<ResponseClienteJson>>("api/Cliente") ?? [];

    public async Task<List<ResponseEstoqueJson>> GetEstoquesAsync()
        => await GetAsync<List<ResponseEstoqueJson>>("api/Estoque") ?? [];

    public async Task<ResponseEstoqueJson> ConfigurarEstoqueAsync(RequestEstoqueJson request)
        => await PutAsync<ResponseEstoqueJson>("api/Estoque", request);

    public async Task<ResponseEstoqueJson> RegistrarEntradaAsync(RequestMovimentacaoEstoqueJson request)
        => await PostAsync<ResponseEstoqueJson>("api/Estoque/entrada", request);

    public async Task<ResponseEstoqueJson> RegistrarSaidaAsync(RequestMovimentacaoEstoqueJson request)
        => await PostAsync<ResponseEstoqueJson>("api/Estoque/saida", request);

    public async Task<ResponseEstoqueJson> AjustarEstoqueAsync(RequestMovimentacaoEstoqueJson request)
        => await PostAsync<ResponseEstoqueJson>("api/Estoque/ajuste", request);

    public async Task<List<ResponseMovimentacaoEstoqueJson>> GetMovimentacoesAsync(Guid? produtoId)
    {
        var path = produtoId.HasValue
            ? $"api/Estoque/movimentacoes?produtoId={produtoId}"
            : "api/Estoque/movimentacoes";

        return await GetAsync<List<ResponseMovimentacaoEstoqueJson>>(path) ?? [];
    }

    public async Task<List<ResponseOrdemServicoJson>> GetOrdensServicoAsync()
        => await GetAsync<List<ResponseOrdemServicoJson>>("api/OrdemServico") ?? [];

    public async Task<ResponseOrdemServicoJson> SaveOrdemServicoAsync(Guid? id, RequestOrdemServicoJson request)
        => id.HasValue
            ? await PutAsync<ResponseOrdemServicoJson>($"api/OrdemServico/{id}", request)
            : await PostAsync<ResponseOrdemServicoJson>("api/OrdemServico", request);

    public Task DeleteOrdemServicoAsync(Guid id) => DeleteAsync($"api/OrdemServico/{id}");

    private async Task<T?> GetAsync<T>(string path)
    {
        using var response = await _httpClient.GetAsync(path);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    private async Task<T> PostAsync<T>(string path, object payload)
    {
        using var response = await _httpClient.PostAsJsonAsync(path, payload);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<T>(_jsonOptions))!;
    }

    private async Task<T> PutAsync<T>(string path, object payload)
    {
        using var response = await _httpClient.PutAsJsonAsync(path, payload);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<T>(_jsonOptions))!;
    }

    private async Task DeleteAsync(string path)
    {
        using var response = await _httpClient.DeleteAsync(path);
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        var message = string.IsNullOrWhiteSpace(body)
            ? response.ReasonPhrase ?? "Falha ao acessar a API."
            : body;

        throw new InvalidOperationException(message);
    }
}
