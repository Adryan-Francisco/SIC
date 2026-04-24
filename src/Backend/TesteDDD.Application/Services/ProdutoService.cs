using AutoMapper;
using Serilog;
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
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IMapper _mapper;

    public ProdutoService(IProdutoRepository produtoRepository, ICategoriaRepository categoriaRepository, IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _categoriaRepository = categoriaRepository;
        _mapper = mapper;
    }

    public async Task<ResponseProdutoJson> CreateAsync(RequestProdutoJson request)
    {
        Log.Information("Iniciando criação de produto: {@Produto}", request);

        // Validar se categoria existe
        var categoria = await _categoriaRepository.GetByIdAsync(request.CategoriaId);
        if (categoria == null)
        {
            Log.Warning("Categoria não encontrada. CategoriaId: {CategoriaId}", request.CategoriaId);
            throw new BusinessRuleException("CATEGORIA_NAO_ENCONTRADA", $"Categoria com ID {request.CategoriaId} não encontrada.");
        }

        var produto = new Produto(request.Nome, request.Preco, request.CategoriaId);

        await _produtoRepository.AddAsync(produto);

        Log.Information("Produto criado com sucesso. ProdutoId: {ProdutoId}, Nome: {Nome}", produto.Id, produto.Nome);

        return _mapper.Map<ResponseProdutoJson>(produto);
    }

    public async Task<IList<ResponseProdutoJson>> GetAllAsync()
    {
        Log.Information("Obtendo todos os produtos");

        var products = await _produtoRepository.GetAllAsync();

        Log.Information("Total de produtos obtidos: {ProductCount}", products.Count());

        return _mapper.Map<IList<ResponseProdutoJson>>(products);
    }

    public async Task<ResponseProdutoJson?> GetByIdAsync(Guid id)
    {
        Log.Debug("Obtendo produto por ID. ProdutoId: {ProdutoId}", id);

        var produto = await _produtoRepository.GetByIdAsync(id);

        if (produto == null)
        {
            Log.Warning("Produto não encontrado. ProdutoId: {ProdutoId}", id);
            return null;
        }

        return _mapper.Map<ResponseProdutoJson>(produto);
    }

    public async Task<ResponseProdutoJson?> UpdateAsync(Guid id, RequestProdutoJson request)
    {
        Log.Information("Atualizando produto. ProdutoId: {ProdutoId}, {@ProdutoData}", id, request);

        var produto = await _produtoRepository.GetByIdAsync(id);

        if (produto == null)
        {
            Log.Warning("Produto não encontrado para atualização. ProdutoId: {ProdutoId}", id);
            return null;
        }

        // Validar se categoria existe
        var categoria = await _categoriaRepository.GetByIdAsync(request.CategoriaId);
        if (categoria == null)
        {
            Log.Warning("Categoria não encontrada na atualização. CategoriaId: {CategoriaId}", request.CategoriaId);
            throw new BusinessRuleException("CATEGORIA_NAO_ENCONTRADA", $"Categoria com ID {request.CategoriaId} não encontrada.");
        }

        produto.Update(request.Nome, request.Preco);

        await _produtoRepository.UpdateAsync(produto);

        Log.Information("Produto atualizado com sucesso. ProdutoId: {ProdutoId}", id);

        return _mapper.Map<ResponseProdutoJson>(produto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        Log.Information("Deletando produto. ProdutoId: {ProdutoId}", id);

        var produto = await _produtoRepository.GetByIdAsync(id);

        if (produto == null)
        {
            Log.Warning("Produto não encontrado para deleção. ProdutoId: {ProdutoId}", id);
            return false;
        }

        await _produtoRepository.DeleteAsync(id);

        Log.Information("Produto deletado com sucesso. ProdutoId: {ProdutoId}", id);

        return true;
    }
}