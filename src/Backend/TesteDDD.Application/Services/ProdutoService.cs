using AutoMapper;
using Serilog;
using TesteDDD.Application.Exceptions;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;

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
    private readonly IFornecedorRepository _fornecedorRepository;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IMapper _mapper;

    public ProdutoService(IProdutoRepository produtoRepository, ICategoriaRepository categoriaRepository, IFornecedorRepository fornecedorRepository, IEstoqueRepository estoqueRepository, IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _categoriaRepository = categoriaRepository;
        _fornecedorRepository = fornecedorRepository;
        _estoqueRepository = estoqueRepository;
        _mapper = mapper;
    }

    public async Task<ResponseProdutoJson> CreateAsync(RequestProdutoJson request)
    {
        Log.Information("Iniciando criacao de produto: {@Produto}", request);
        Validate(request);

        await EnsureReferencesAsync(request.CategoriaId, request.FornecedorId);

        var produto = new Produto(request.Nome, request.Preco, request.CategoriaId, request.FornecedorId);
        await _produtoRepository.AddAsync(produto);

        var estoque = new Estoque(produto.Id, 0, 0);
        await _estoqueRepository.AddAsync(estoque);

        Log.Information("Produto criado com sucesso. ProdutoId: {ProdutoId}, Nome: {Nome}", produto.Id, produto.Nome);

        var saved = await _produtoRepository.GetByIdAsync(produto.Id) ?? produto;
        return _mapper.Map<ResponseProdutoJson>(saved);
    }

    public async Task<IList<ResponseProdutoJson>> GetAllAsync()
    {
        var products = await _produtoRepository.GetAllAsync();
        return _mapper.Map<IList<ResponseProdutoJson>>(products);
    }

    public async Task<ResponseProdutoJson?> GetByIdAsync(Guid id)
    {
        var produto = await _produtoRepository.GetByIdAsync(id);
        return produto == null ? null : _mapper.Map<ResponseProdutoJson>(produto);
    }

    public async Task<ResponseProdutoJson?> UpdateAsync(Guid id, RequestProdutoJson request)
    {
        Validate(request);

        var produto = await _produtoRepository.GetByIdAsync(id);
        if (produto == null)
            return null;

        await EnsureReferencesAsync(request.CategoriaId, request.FornecedorId);

        produto.Update(request.Nome, request.Preco, request.CategoriaId, request.FornecedorId);
        await _produtoRepository.UpdateAsync(produto);

        return _mapper.Map<ResponseProdutoJson>(produto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var produto = await _produtoRepository.GetByIdAsync(id);
        if (produto == null)
            return false;

        await _produtoRepository.DeleteAsync(id);
        return true;
    }

    private async Task EnsureReferencesAsync(Guid categoriaId, Guid fornecedorId)
    {
        if (await _categoriaRepository.GetByIdAsync(categoriaId) == null)
            throw new BusinessRuleException("CATEGORIA_NAO_ENCONTRADA", $"Categoria com ID {categoriaId} nao encontrada.");

        if (await _fornecedorRepository.GetByIdAsync(fornecedorId) == null)
            throw new BusinessRuleException("FORNECEDOR_NAO_ENCONTRADO", $"Fornecedor com ID {fornecedorId} nao encontrado.");
    }

    private static void Validate(RequestProdutoJson request)
    {
        if (!string.IsNullOrWhiteSpace(request.Nome) && request.Nome.Length < 3)
            throw new BusinessRuleException("PRODUTO_NOME_CURTO", "Nome do produto deve ter pelo menos 3 caracteres.");
    }
}
