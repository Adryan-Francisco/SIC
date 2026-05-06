using AutoMapper;
using Microsoft.Extensions.Logging;
using TesteDDD.Application.Exceptions;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Sefaz;

namespace TesteDDD.Application.Services;

public interface INotaFiscalService
{
    Task<ResponseNotaFiscalJson> EmitirAsync(RequestEmitirNotaFiscalJson request);
    Task<ResponseNotaFiscalJson> ObterPorIdAsync(Guid id);
    Task<ResponseNotaFiscalJson> ConsultarStatusAsync(Guid id);
    Task<ResponseNotaFiscalJson> CancelarAsync(Guid id, RequestCancelarNotaFiscalJson request);
    Task<List<ResponseNotaFiscalJson>> ListarPorStatusAsync(int status);
}

public class NotaFiscalService : INotaFiscalService
{
    private readonly INotaFiscalRepository _notaFiscalRepository;
    private readonly IVendasRepository _vendasRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IItemVendaRepository _itemVendasRepository;
    private readonly ISefazIntegrationService _sefazService;
    private readonly IXmlGeracaoNfeService _xmlGeracaoService;
    private readonly SefazIntegrationSettings _sefazSettings;
    private readonly IMapper _mapper;
    private readonly ILogger<NotaFiscalService> _logger;

    public NotaFiscalService(
        INotaFiscalRepository notaFiscalRepository,
        IVendasRepository vendasRepository,
        IClienteRepository clienteRepository,
        IItemVendaRepository itemVendasRepository,
        ISefazIntegrationService sefazService,
        IXmlGeracaoNfeService xmlGeracaoService,
        SefazIntegrationSettings sefazSettings,
        IMapper mapper,
        ILogger<NotaFiscalService> logger)
    {
        _notaFiscalRepository = notaFiscalRepository ?? throw new ArgumentNullException(nameof(notaFiscalRepository));
        _vendasRepository = vendasRepository ?? throw new ArgumentNullException(nameof(vendasRepository));
        _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
        _itemVendasRepository = itemVendasRepository ?? throw new ArgumentNullException(nameof(itemVendasRepository));
        _sefazService = sefazService ?? throw new ArgumentNullException(nameof(sefazService));
        _xmlGeracaoService = xmlGeracaoService ?? throw new ArgumentNullException(nameof(xmlGeracaoService));
        _sefazSettings = sefazSettings ?? throw new ArgumentNullException(nameof(sefazSettings));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ResponseNotaFiscalJson> EmitirAsync(RequestEmitirNotaFiscalJson request)
    {
        try
        {
            _logger.LogInformation("Iniciando emissão de nota fiscal para venda: {VendasId}", request.VendasId);

            if (request.VendasId == Guid.Empty)
                throw new InvalidOperationException("VendasId não pode estar vazio.");

            if (request.Serie <= 0 || request.Numero <= 0)
                throw new InvalidOperationException("Série e número devem ser maiores que zero.");

            var venda = await _vendasRepository.GetByIdAsync(request.VendasId);
            if (venda == null)
                throw new NotFoundException("Venda não encontrada.");

            var notaExistente = await _notaFiscalRepository.ObterPorVendasIdAsync(request.VendasId);
            if (notaExistente != null)
                throw new InvalidOperationException("Já existe uma nota fiscal para esta venda.");

            // TODO: associar o cliente à venda no domínio; por enquanto usamos o primeiro cliente disponível.
            var cliente = (await _clienteRepository.GetAllAsync()).FirstOrDefault();
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            var itens = venda.Itens;
            if (itens == null || itens.Count == 0)
                throw new InvalidOperationException("Venda sem itens não pode ter nota fiscal emitida.");

            var notaFiscal = new NotaFiscal(request.VendasId, request.Serie, request.Numero);
            notaFiscal.MarcarComoPendente();

            var xmlNota = _xmlGeracaoService.GerarXmlNFe(
                notaFiscal,
                venda,
                cliente,
                itens,
                _sefazSettings.CnpjEmitente ?? string.Empty);

            var respostaAutorizacao = await _sefazService.AutorizarNotaAsync(xmlNota);

            if (respostaAutorizacao.Sucesso)
            {
                notaFiscal.AtualizarChaveAcesso(respostaAutorizacao.ChaveAcesso);
                notaFiscal.MarcarComoAutorizada(respostaAutorizacao.Protocolo, respostaAutorizacao.XmlAutorizado);
                notaFiscal.DefinirDanfeUrl(GerarUrlDanfe(respostaAutorizacao.ChaveAcesso));

                _logger.LogInformation("Nota fiscal autorizada: {ChaveAcesso}", respostaAutorizacao.ChaveAcesso);
            }
            else
            {
                notaFiscal.MarcarComoRejeitada(respostaAutorizacao.Mensagem);
                _logger.LogWarning("Falha ao autorizar nota fiscal: {Mensagem}", respostaAutorizacao.Mensagem);
            }

            await _notaFiscalRepository.AdicionarAsync(notaFiscal);
            return _mapper.Map<ResponseNotaFiscalJson>(notaFiscal);
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao emitir nota fiscal: {Mensagem}", ex.Message);
            throw;
        }
    }

    public async Task<ResponseNotaFiscalJson> ObterPorIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new InvalidOperationException("Id não pode estar vazio.");

        var notaFiscal = await _notaFiscalRepository.ObterPorIdAsync(id);
        if (notaFiscal == null)
            throw new NotFoundException("Nota fiscal não encontrada.");

        return _mapper.Map<ResponseNotaFiscalJson>(notaFiscal);
    }

    public async Task<ResponseNotaFiscalJson> ConsultarStatusAsync(Guid id)
    {
        try
        {
            var notaFiscal = await _notaFiscalRepository.ObterPorIdAsync(id);
            if (notaFiscal == null)
                throw new NotFoundException("Nota fiscal não encontrada.");

            if (notaFiscal.Status == NotaFiscalStatus.Pendente)
            {
                var respostaConsulta = await _sefazService.ConsultarStatusAsync(notaFiscal.ChaveAcesso);

                if (respostaConsulta.Sucesso && respostaConsulta.Status == "100")
                {
                    notaFiscal.MarcarComoAutorizada(respostaConsulta.Protocolo, string.Empty);
                    await _notaFiscalRepository.AtualizarAsync(notaFiscal);
                }
            }

            return _mapper.Map<ResponseNotaFiscalJson>(notaFiscal);
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao consultar status: {Mensagem}", ex.Message);
            throw;
        }
    }

    public async Task<ResponseNotaFiscalJson> CancelarAsync(Guid id, RequestCancelarNotaFiscalJson request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Justificativa) || request.Justificativa.Length < 15)
                throw new InvalidOperationException("Justificativa deve ter no mínimo 15 caracteres.");

            var notaFiscal = await _notaFiscalRepository.ObterPorIdAsync(id);
            if (notaFiscal == null)
                throw new NotFoundException("Nota fiscal não encontrada.");

            if (notaFiscal.Status != NotaFiscalStatus.Autorizada)
                throw new InvalidOperationException("Apenas notas autorizadas podem ser canceladas.");

            var respostaCancelamento = await _sefazService.CancelarNotaAsync(notaFiscal.ChaveAcesso, request.Justificativa);

            if (respostaCancelamento.Sucesso)
            {
                notaFiscal.MarcarComoCancelada();
                await _notaFiscalRepository.AtualizarAsync(notaFiscal);
                _logger.LogInformation("Nota fiscal cancelada: {ChaveAcesso}", notaFiscal.ChaveAcesso);
            }
            else
            {
                throw new InvalidOperationException($"Erro ao cancelar nota no Sefaz: {respostaCancelamento.Mensagem}");
            }

            return _mapper.Map<ResponseNotaFiscalJson>(notaFiscal);
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao cancelar nota fiscal: {Mensagem}", ex.Message);
            throw;
        }
    }

    public async Task<List<ResponseNotaFiscalJson>> ListarPorStatusAsync(int status)
    {
        var notasFiscais = await _notaFiscalRepository.ListarPorStatusAsync(status);
        return _mapper.Map<List<ResponseNotaFiscalJson>>(notasFiscais);
    }

    private string GerarUrlDanfe(string chaveAcesso)
    {
        return $"https://nfe.fazenda.sp.gov.br/danfeweb/consultar?chNFe={chaveAcesso}";
    }
}
