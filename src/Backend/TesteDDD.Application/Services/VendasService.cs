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
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;

    public VendasService(IVendasRepository repository, IProdutoRepository produtoRepository, IEstoqueRepository estoqueRepository, IMovimentacaoEstoqueRepository movimentacaoRepository)
    {
        _repository = repository;
        _produtoRepository = produtoRepository;
        _estoqueRepository = estoqueRepository;
        _movimentacaoRepository = movimentacaoRepository;
    }

    public async Task<ResponseVendasJson> CreateAsync(RequestVendasJson request)
    {
        var itens = await BuildItensAsync(request);
        await ApplyStockOutputAsync(itens, $"Saida por venda em {request.DataVenda:yyyy-MM-dd}");

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

        await RestoreStockAsync(venda.Itens, $"Estorno de estoque por atualizacao da venda {venda.Id}");

        var itens = await BuildItensAsync(request);
        await ApplyStockOutputAsync(itens, $"Saida por atualizacao da venda {venda.Id}");

        venda.Update(request.DataVenda, itens);
        await _repository.UpdateAsync(venda);

        return Map(venda);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var venda = await _repository.GetByIdAsync(id);
        if (venda == null)
            return false;

        await RestoreStockAsync(venda.Itens, $"Estorno de estoque por exclusao da venda {venda.Id}");
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

    private async Task ApplyStockOutputAsync(IEnumerable<ItemVendas> itens, string observacao)
    {
        foreach (var item in itens)
        {
            var estoque = await _estoqueRepository.GetByProdutoIdAsync(item.ProdutoId);
            if (estoque == null)
                throw new BusinessRuleException("ESTOQUE_NAO_CONFIGURADO", $"Estoque do produto {item.ProdutoId} nao configurado.");

            var quantidadeAnterior = estoque.QuantidadeDisponivel;
            estoque.RegistrarSaida(item.Quantidade);
            await _estoqueRepository.UpdateAsync(estoque);

            var movimentacao = new MovimentacaoEstoque(item.ProdutoId, TipoMovimentacaoEstoque.Saida, item.Quantidade, quantidadeAnterior, estoque.QuantidadeDisponivel, observacao);
            await _movimentacaoRepository.AddAsync(movimentacao);
        }
    }

    private async Task RestoreStockAsync(IEnumerable<ItemVendas> itens, string observacao)
    {
        foreach (var item in itens)
        {
            var estoque = await _estoqueRepository.GetByProdutoIdAsync(item.ProdutoId);
            if (estoque == null)
                throw new BusinessRuleException("ESTOQUE_NAO_CONFIGURADO", $"Estoque do produto {item.ProdutoId} nao configurado.");

            var quantidadeAnterior = estoque.QuantidadeDisponivel;
            estoque.RegistrarEntrada(item.Quantidade);
            await _estoqueRepository.UpdateAsync(estoque);

            var movimentacao = new MovimentacaoEstoque(item.ProdutoId, TipoMovimentacaoEstoque.Entrada, item.Quantidade, quantidadeAnterior, estoque.QuantidadeDisponivel, observacao);
            await _movimentacaoRepository.AddAsync(movimentacao);
        }
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
