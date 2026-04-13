using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Application.Exceptions;

namespace TesteDDD.Application.Services;

public interface IProdutoService
{
    Task<ResponseProdutoJson> CreateAsync(RequestProdutoJson request);
    Task<IList<ResponseProdutoJson>> GetAllAsync();
    Task<ResponseProdutoJson?> GetByIdAsync(Guid id);
    Task<ResponseProdutoJson?> UpdateAsync(Guid id, RequestProdutoJson request);
    Task<bool> DeleteAsync(Guid id);
}

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repository;
    private readonly ICategoriaRepository _categoriaRepository;

    public ProdutoService(IProdutoRepository repository, ICategoriaRepository categoriaRepository)
    {
        _repository = repository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<ResponseProdutoJson> CreateAsync(RequestProdutoJson request)
    {
        await ValidateRequestAsync(request);

        var produto = new Produto(request.Nome, request.Preco, request.CategoriaId);

        await _repository.AddAsync(produto);

        return new ResponseProdutoJson
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            CategoriaId = produto.CategoriaId
        };
    }

    public async Task<IList<ResponseProdutoJson>> GetAllAsync()
    {
        var products = await _repository.GetAllAsync();

        return products.Select(p => new ResponseProdutoJson
        {
            Id = p.Id,
            Nome = p.Nome,
            Preco = p.Preco,
            CategoriaId = p.CategoriaId
        }).ToList();
    }

    public async Task<ResponseProdutoJson?> GetByIdAsync(Guid id)
    {
        var produto = await _repository.GetByIdAsync(id);

        if (produto == null)
            return null;

        return new ResponseProdutoJson
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            CategoriaId = produto.CategoriaId
        };
    }

    public async Task<ResponseProdutoJson?> UpdateAsync(Guid id, RequestProdutoJson request)
    {
        await ValidateRequestAsync(request);

        var produto = await _repository.GetByIdAsync(id);

        if (produto == null)
            return null;

        produto.Update(request.Nome, request.Preco, request.CategoriaId);

        await _repository.UpdateAsync(produto);

        return new ResponseProdutoJson
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            CategoriaId = produto.CategoriaId
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var produto = await _repository.GetByIdAsync(id);

        if (produto == null)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    private async Task ValidateRequestAsync(RequestProdutoJson request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new BusinessRuleException("PRODUTO_NOME_INVALIDO", "Nome do produto e obrigatorio.");

        if (request.Nome.Trim().Length < 3)
            throw new BusinessRuleException("PRODUTO_NOME_CURTO", "Nome do produto deve ter pelo menos 3 caracteres.");

        if (request.Preco <= 0)
            throw new BusinessRuleException("PRODUTO_PRECO_INVALIDO", "Preco do produto deve ser maior que zero.");

        if (request.CategoriaId == Guid.Empty)
            throw new BusinessRuleException("PRODUTO_CATEGORIA_INVALIDA", "Categoria do produto e obrigatoria.");

        var categoria = await _categoriaRepository.GetByIdAsync(request.CategoriaId);
        if (categoria == null)
            throw new BusinessRuleException("PRODUTO_CATEGORIA_NAO_ENCONTRADA", "Categoria informada nao foi encontrada.");
    }
}
