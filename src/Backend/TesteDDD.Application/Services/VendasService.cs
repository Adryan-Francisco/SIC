using TesteDDD.Application.Exceptions;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;

namespace TesteDDD.Application.Services;

public interface IVendasService
{
    Task<ResponseVendasJson> CreateAsync(RequestVendasJson request);
    Task<IList<ResponseVendasJson>> GetAllAsync();
    Task<ResponseVendasJson?> GetByIdAsync(Guid id);
    Task<ResponseVendasJson?> UpdateAsync(Guid id, RequestVendasJson request);
    Task<bool> DeleteAsync(Guid id);
}

public class VendasService : IVendasService
{
    private readonly IVendasRepository _repository;
    private readonly IProdutoRepository _produtoRepository;

    public VendasService(IVendasRepository repository, IProdutoRepository produtoRepository)
    {
        _repository = repository;
        _produtoRepository = produtoRepository;
    }

    public async Task<ResponseVendasJson> CreateAsync(RequestVendasJson request)
    {
        var itens = await BuildItensAsync(request);
        var venda = new Vendas(request.DataVenda, itens);
        await _repository.AddAsync(venda);
        return Map(venda);
    }

    public async Task<IList<ResponseVendasJson>> GetAllAsync()
    {
        var vendas = await _repository.GetAllAsync();
        return vendas.Select(Map).ToList();
    }

    public async Task<ResponseVendasJson?> GetByIdAsync(Guid id)
    {
        var venda = await _repository.GetByIdAsync(id);
        return venda == null ? null : Map(venda);
    }

    public async Task<ResponseVendasJson?> UpdateAsync(Guid id, RequestVendasJson request)
    {
        var venda = await _repository.GetByIdAsync(id);
        if (venda == null)
            return null;

        var itens = await BuildItensAsync(request);
        venda.Update(request.DataVenda, itens);
        await _repository.UpdateAsync(venda);

        return Map(venda);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var venda = await _repository.GetByIdAsync(id);
        if (venda == null)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    private async Task<List<ItemVendas>> BuildItensAsync(RequestVendasJson request)
    {
        if (request.Itens == null || request.Itens.Count == 0)
            throw new BusinessRuleException("VENDA_ITENS_INVALIDOS", "A venda deve conter pelo menos um item.");

        var itens = new List<ItemVendas>();

        foreach (var itemRequest in request.Itens)
        {
            var produto = await _produtoRepository.GetByIdAsync(itemRequest.IdProduto);
            if (produto == null)
                throw new BusinessRuleException("VENDA_PRODUTO_NAO_ENCONTRADO", $"Produto com ID {itemRequest.IdProduto} nao encontrado.");

            var item = new ItemVendas(produto.Id, itemRequest.Quantidade, itemRequest.PrecoUnitario);
            item.DefinirProduto(produto);
            itens.Add(item);
        }

        return itens;
    }

    private static ResponseVendasJson Map(Vendas venda)
    {
        return new ResponseVendasJson
        {
            Id = venda.Id,
            DataVenda = venda.DataVenda,
            Total = venda.ValorTotal,
            Itens = venda.Itens.Select(item => new ResponseItemVendasJson
            {
                Id = item.Id,
                ProdutoId = item.ProdutoId,
                ProdutoNome = item.Produto?.Nome ?? string.Empty,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario
            }).ToList()
        };
    }
}
