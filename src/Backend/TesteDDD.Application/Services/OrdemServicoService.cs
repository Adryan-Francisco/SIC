using AutoMapper;
using TesteDDD.Application.Exceptions;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;

namespace TesteDDD.Application.Services;

public interface IOrdemServicoService
{
    Task<ResponseOrdemServicoJson> CreateAsync(RequestOrdemServicoJson request);
    Task<IList<ResponseOrdemServicoJson>> GetAllAsync();
    Task<ResponseOrdemServicoJson?> GetByIdAsync(Guid id);
    Task<ResponseOrdemServicoJson?> UpdateAsync(Guid id, RequestOrdemServicoJson request);
    Task<bool> DeleteAsync(Guid id);
}

public class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMapper _mapper;

    public OrdemServicoService(IOrdemServicoRepository repository, IClienteRepository clienteRepository, IProdutoRepository produtoRepository, IMapper mapper)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _produtoRepository = produtoRepository;
        _mapper = mapper;
    }

    public async Task<ResponseOrdemServicoJson> CreateAsync(RequestOrdemServicoJson request)
    {
        await EnsureReferencesAsync(request.ClienteId, request.ProdutoId);

        var ordemServico = new OrdemServico(request.ClienteId, request.ProdutoId, request.Descricao, request.DataAbertura, request.ValorServico);
        ApplyStatus(ordemServico, request.Status, request.DataConclusao);

        await _repository.AddAsync(ordemServico);

        var saved = await _repository.GetByIdAsync(ordemServico.Id) ?? ordemServico;
        return _mapper.Map<ResponseOrdemServicoJson>(saved);
    }

    public async Task<IList<ResponseOrdemServicoJson>> GetAllAsync()
    {
        var ordens = await _repository.GetAllAsync();
        return _mapper.Map<IList<ResponseOrdemServicoJson>>(ordens);
    }

    public async Task<ResponseOrdemServicoJson?> GetByIdAsync(Guid id)
    {
        var ordem = await _repository.GetByIdAsync(id);
        return ordem == null ? null : _mapper.Map<ResponseOrdemServicoJson>(ordem);
    }

    public async Task<ResponseOrdemServicoJson?> UpdateAsync(Guid id, RequestOrdemServicoJson request)
    {
        var ordemServico = await _repository.GetByIdAsync(id);
        if (ordemServico == null)
            return null;

        await EnsureReferencesAsync(request.ClienteId, request.ProdutoId);

        ordemServico.Update(request.ClienteId, request.ProdutoId, request.Descricao, request.DataAbertura, request.ValorServico);
        ApplyStatus(ordemServico, request.Status, request.DataConclusao);

        await _repository.UpdateAsync(ordemServico);
        return _mapper.Map<ResponseOrdemServicoJson>(ordemServico);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var ordem = await _repository.GetByIdAsync(id);
        if (ordem == null)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    private async Task EnsureReferencesAsync(Guid clienteId, Guid produtoId)
    {
        if (await _clienteRepository.GetByIdAsync(clienteId) == null)
            throw new BusinessRuleException("ORDEM_SERVICO_CLIENTE_NAO_ENCONTRADO", $"Cliente com ID {clienteId} nao encontrado.");

        if (await _produtoRepository.GetByIdAsync(produtoId) == null)
            throw new BusinessRuleException("ORDEM_SERVICO_PRODUTO_NAO_ENCONTRADO", $"Produto com ID {produtoId} nao encontrado.");
    }

    private static void ApplyStatus(OrdemServico ordemServico, int status, DateTime? dataConclusao)
    {
        var statusDesejado = (OrdemServicoStatus)status;

        if (statusDesejado == OrdemServicoStatus.EmAndamento && ordemServico.Status == OrdemServicoStatus.Aberta)
            ordemServico.Iniciar();

        if (statusDesejado == OrdemServicoStatus.Concluida && ordemServico.Status != OrdemServicoStatus.Concluida)
            ordemServico.Concluir(dataConclusao ?? DateTime.UtcNow);

        if (statusDesejado == OrdemServicoStatus.Cancelada && ordemServico.Status != OrdemServicoStatus.Cancelada)
            ordemServico.Cancelar();
    }
}
