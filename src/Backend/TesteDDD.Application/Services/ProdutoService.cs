using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;

namespace TesteDDD.Application.Services;

public interface IProdutoService
{
    Task<ResponseProdutoJson> CreateAsync(RequestProdutoJson request);
    Task<IEnumerable<ResponseProdutoJson>> GetAllAsync();
    // Adicione os métodos GetById, Update e Delete na interface...
}

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repository;

    public ProdutoService(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseProdutoJson> CreateAsync(RequestProdutoJson request)
    {
        // 1. Mapear Request para Entidade
        var produto = new Produto(request.Nome, request.Preco);

        // 2. Persistir
        await _repository.AddAsync(produto);

        // 3. Retornar Response
        return new ResponseProdutoJson
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco
        };
    }

    public async Task<IEnumerable<ResponseProdutoJson>> GetAllAsync()
    {
        var products = await _repository.GetAllAsync();

        return products.Select(p => new ResponseProdutoJson
        {
            Id = p.Id,
            Nome = p.Nome,
            Preco = p.Preco
        });
    }

    // Implemente os demais métodos (Update, Delete, GetById) seguindo essa lógica...
}