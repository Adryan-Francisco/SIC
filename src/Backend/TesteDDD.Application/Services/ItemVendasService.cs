using TesteDDD.Application.Exceptions;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;

namespace TesteDDD.Application.Services;

public interface IItemVendasService
{
    Task<ResponseItemVendasJson> CreateAsync(RequestItemVendasJson request);
    Task<IList<ResponseItemVendasJson>> GetAllAsync();
    Task<ResponseItemVendasJson?> GetByIdAsync(Guid id);
    Task<ResponseItemVendasJson?> UpdateAsync(Guid id, RequestItemVendasJson request);
    Task<bool> DeleteAsync(Guid id);
}

public class ItemVendasService : IItemVendasService
{
    private readonly IItemVendaRepository _repository;
    private readonly IProdutoRepository _produtoRepository;

    public ItemVendasService(IItemVendaRepository repository, IProdutoRepository produtoRepository)
    {
        _repository = repository;
        _produtoRepository = produtoRepository;
    }

    public async Task<ResponseItemVendasJson> CreateAsync(RequestItemVendasJson request)
    {
        var produto = await GetProdutoAsync(request.IdProduto);
        var itemVenda = new ItemVendas(produto.Id, request.Quantidade, request.PrecoUnitario);
        itemVenda.DefinirProduto(produto);
        await _repository.AddAsync(itemVenda);
        return Map(itemVenda);
    }

    public async Task<IList<ResponseItemVendasJson>> GetAllAsync()
    {
        var itens = await _repository.GetAllAsync();
        return itens.Select(Map).ToList();
    }

    public async Task<ResponseItemVendasJson?> GetByIdAsync(Guid id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item == null ? null : Map(item);
    }

    public async Task<ResponseItemVendasJson?> UpdateAsync(Guid id, RequestItemVendasJson request)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
            return null;

        if (item.ProdutoId != request.IdProduto)
        {
            var produto = await GetProdutoAsync(request.IdProduto);
            item.DefinirProduto(produto);
        }

        item.Update(request.Quantidade, request.PrecoUnitario);
        await _repository.UpdateAsync(item);
        return Map(item);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    private async Task<Produto> GetProdutoAsync(Guid produtoId)
    {
        if (produtoId == Guid.Empty)
            throw new BusinessRuleException("ITEM_VENDA_PRODUTO_INVALIDO", "Produto do item de venda e obrigatorio.");

        var produto = await _produtoRepository.GetByIdAsync(produtoId);
        if (produto == null)
            throw new BusinessRuleException("ITEM_VENDA_PRODUTO_NAO_ENCONTRADO", $"Produto com ID {produtoId} nao encontrado.");

        return produto;
    }

    private static ResponseItemVendasJson Map(ItemVendas item)
    {
        return new ResponseItemVendasJson
        {
            Id = item.Id,
            ProdutoId = item.ProdutoId,
            ProdutoNome = item.Produto?.Nome ?? string.Empty,
            Quantidade = item.Quantidade,
            PrecoUnitario = item.PrecoUnitario
        };
    }
}
