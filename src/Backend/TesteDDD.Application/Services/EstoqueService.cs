using AutoMapper;
using TesteDDD.Application.Exceptions;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;

namespace TesteDDD.Application.Services;

public interface IEstoqueService
{
    Task<ResponseEstoqueJson> ConfigurarAsync(RequestEstoqueJson request);
    Task<IList<ResponseEstoqueJson>> GetAllAsync();
    Task<ResponseEstoqueJson?> GetByProdutoIdAsync(Guid produtoId);
    Task<ResponseEstoqueJson> RegistrarEntradaAsync(RequestMovimentacaoEstoqueJson request);
    Task<ResponseEstoqueJson> RegistrarSaidaAsync(RequestMovimentacaoEstoqueJson request);
    Task<ResponseEstoqueJson> AjustarAsync(RequestMovimentacaoEstoqueJson request);
    Task<IList<ResponseMovimentacaoEstoqueJson>> GetMovimentacoesAsync(Guid? produtoId);
}

public class EstoqueService : IEstoqueService
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMapper _mapper;

    public EstoqueService(IEstoqueRepository estoqueRepository, IMovimentacaoEstoqueRepository movimentacaoRepository, IProdutoRepository produtoRepository, IMapper mapper)
    {
        _estoqueRepository = estoqueRepository;
        _movimentacaoRepository = movimentacaoRepository;
        _produtoRepository = produtoRepository;
        _mapper = mapper;
    }

    public async Task<ResponseEstoqueJson> ConfigurarAsync(RequestEstoqueJson request)
    {
        var produto = await _produtoRepository.GetByIdAsync(request.ProdutoId)
            ?? throw new BusinessRuleException("ESTOQUE_PRODUTO_NAO_ENCONTRADO", $"Produto com ID {request.ProdutoId} nao encontrado.");

        var estoque = await _estoqueRepository.GetByProdutoIdAsync(request.ProdutoId);
        if (estoque == null)
        {
            estoque = new Estoque(request.ProdutoId, request.QuantidadeDisponivel, request.QuantidadeMinima);
            await _estoqueRepository.AddAsync(estoque);
        }
        else
        {
            estoque.Configurar(request.QuantidadeDisponivel, request.QuantidadeMinima);
            await _estoqueRepository.UpdateAsync(estoque);
        }

        return _mapper.Map<ResponseEstoqueJson>(estoque);
    }

    public async Task<IList<ResponseEstoqueJson>> GetAllAsync()
    {
        var estoques = await _estoqueRepository.GetAllAsync();
        return _mapper.Map<IList<ResponseEstoqueJson>>(estoques);
    }

    public async Task<ResponseEstoqueJson?> GetByProdutoIdAsync(Guid produtoId)
    {
        var estoque = await _estoqueRepository.GetByProdutoIdAsync(produtoId);
        return estoque == null ? null : _mapper.Map<ResponseEstoqueJson>(estoque);
    }

    public async Task<ResponseEstoqueJson> RegistrarEntradaAsync(RequestMovimentacaoEstoqueJson request)
    {
        var estoque = await GetRequiredEstoqueAsync(request.ProdutoId);
        var quantidadeAnterior = estoque.QuantidadeDisponivel;

        estoque.RegistrarEntrada(request.Quantidade);
        await _estoqueRepository.UpdateAsync(estoque);
        await RegisterMovementAsync(request.ProdutoId, TipoMovimentacaoEstoque.Entrada, request.Quantidade, quantidadeAnterior, estoque.QuantidadeDisponivel, request.Observacao);

        return _mapper.Map<ResponseEstoqueJson>(estoque);
    }

    public async Task<ResponseEstoqueJson> RegistrarSaidaAsync(RequestMovimentacaoEstoqueJson request)
    {
        var estoque = await GetRequiredEstoqueAsync(request.ProdutoId);
        var quantidadeAnterior = estoque.QuantidadeDisponivel;

        estoque.RegistrarSaida(request.Quantidade);
        await _estoqueRepository.UpdateAsync(estoque);
        await RegisterMovementAsync(request.ProdutoId, TipoMovimentacaoEstoque.Saida, request.Quantidade, quantidadeAnterior, estoque.QuantidadeDisponivel, request.Observacao);

        return _mapper.Map<ResponseEstoqueJson>(estoque);
    }

    public async Task<ResponseEstoqueJson> AjustarAsync(RequestMovimentacaoEstoqueJson request)
    {
        var estoque = await GetRequiredEstoqueAsync(request.ProdutoId);
        var quantidadeAnterior = estoque.QuantidadeDisponivel;

        estoque.Ajustar(request.Quantidade);
        await _estoqueRepository.UpdateAsync(estoque);
        await RegisterMovementAsync(request.ProdutoId, TipoMovimentacaoEstoque.Ajuste, Math.Abs(estoque.QuantidadeDisponivel - quantidadeAnterior), quantidadeAnterior, estoque.QuantidadeDisponivel, request.Observacao);

        return _mapper.Map<ResponseEstoqueJson>(estoque);
    }

    public async Task<IList<ResponseMovimentacaoEstoqueJson>> GetMovimentacoesAsync(Guid? produtoId)
    {
        var movimentacoes = produtoId.HasValue
            ? await _movimentacaoRepository.GetByProdutoIdAsync(produtoId.Value)
            : await _movimentacaoRepository.GetAllAsync();

        return _mapper.Map<IList<ResponseMovimentacaoEstoqueJson>>(movimentacoes);
    }

    private async Task<Estoque> GetRequiredEstoqueAsync(Guid produtoId)
    {
        var estoque = await _estoqueRepository.GetByProdutoIdAsync(produtoId);
        if (estoque == null)
            throw new BusinessRuleException("ESTOQUE_NAO_CONFIGURADO", $"Estoque do produto {produtoId} nao configurado.");

        return estoque;
    }

    private async Task RegisterMovementAsync(Guid produtoId, TipoMovimentacaoEstoque tipo, int quantidade, int quantidadeAnterior, int quantidadeAtual, string observacao)
    {
        if (quantidade == 0)
            return;

        var movimentacao = new MovimentacaoEstoque(produtoId, tipo, quantidade, quantidadeAnterior, quantidadeAtual, observacao);
        await _movimentacaoRepository.AddAsync(movimentacao);
    }
}
