using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Application.Exceptions;
using TesteDDD.Application.Services;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Infrastructure.Sefaz;
using AutoMapper;

namespace TesteDDD.Api.IntegrationTests;

public class NotaFiscalIntegrationTests
{
    private readonly Mock<INotaFiscalRepository> _notaFiscalRepositoryMock;
    private readonly Mock<IVendasRepository> _vendasRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ISefazIntegrationService> _sefazServiceMock;
    private readonly Mock<IXmlGeracaoNfeService> _xmlGeracaoServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<NotaFiscalService>> _loggerMock;
    private readonly SefazIntegrationSettings _sefazSettings;
    private readonly NotaFiscalService _notaFiscalService;

    public NotaFiscalIntegrationTests()
    {
        _notaFiscalRepositoryMock = new Mock<INotaFiscalRepository>();
        _vendasRepositoryMock = new Mock<IVendasRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _sefazServiceMock = new Mock<ISefazIntegrationService>();
        _xmlGeracaoServiceMock = new Mock<IXmlGeracaoNfeService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<NotaFiscalService>>();

        _sefazSettings = new SefazIntegrationSettings
        {
            UrlProducao = "https://e-nfe.sefaz.rs.gov.br/webservices",
            UrlHomologacao = "https://nfe-homolog.svrs.rs.gov.br/webservices",
            UtilizarHomologacao = true,
            CnpjEmitente = "00.000.000/0000-00",
            Uf = "SP"
        };

        _notaFiscalService = new NotaFiscalService(
            _notaFiscalRepositoryMock.Object,
            _vendasRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _sefazServiceMock.Object,
            _xmlGeracaoServiceMock.Object,
            _sefazSettings,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task EmitirAsync_ComDadosValidos_DeveRetornarNotaAutorizada()
    {
        // Arrange
        var vendaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();

        var request = new RequestEmitirNotaFiscalJson
        {
            VendasId = vendaId,
            ClienteId = clienteId,
            Serie = 1,
            Numero = 100
        };

        var produto = new Produto("Teste Produto", 100, Guid.NewGuid());
        var itemVenda = new ItemVendas(produtoId, 2, 100);
        itemVenda.DefinirProduto(produto);

        var venda = new Vendas(DateTime.UtcNow, new List<ItemVendas> { itemVenda });

        var cliente = new Cliente(clienteId, "Cliente Teste", "Rua Teste, 123", "01310100");

        var notaFiscalEsperada = new NotaFiscal(vendaId, 1, 100);
        notaFiscalEsperada.AtualizarChaveAcesso("35260401123456789012345678901234567890123");
        notaFiscalEsperada.MarcarComoAutorizada("2026040100012345", "<xml>autorizado</xml>");

        _vendasRepositoryMock.Setup(r => r.GetByIdAsync(vendaId))
            .ReturnsAsync(venda);

        _clienteRepositoryMock.Setup(r => r.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        _notaFiscalRepositoryMock.Setup(r => r.ObterPorVendasIdAsync(vendaId))
            .ReturnsAsync((NotaFiscal?)null);

        _xmlGeracaoServiceMock.Setup(s => s.GerarXmlNFe(It.IsAny<NotaFiscal>(), It.IsAny<Vendas>(), 
            It.IsAny<Cliente>(), It.IsAny<List<ItemVendas>>(), It.IsAny<string>()))
            .Returns("<xml>nfe</xml>");

        _sefazServiceMock.Setup(s => s.AutorizarNotaAsync(It.IsAny<string>()))
            .ReturnsAsync(new SefazAutorizacaoResponse
            {
                Sucesso = true,
                Protocolo = "2026040100012345",
                ChaveAcesso = "35260401123456789012345678901234567890123",
                XmlAutorizado = "<xml>autorizado</xml>",
                Mensagem = "Autorizada com sucesso"
            });

        _notaFiscalRepositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<NotaFiscal>()))
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(m => m.Map<ResponseNotaFiscalJson>(It.IsAny<NotaFiscal>()))
            .Returns(new ResponseNotaFiscalJson
            {
                Id = notaFiscalEsperada.Id,
                ChaveAcesso = "35260401123456789012345678901234567890123",
                Status = 3,
                StatusNome = "Autorizada",
                ProtocoloAutorizacao = "2026040100012345"
            });

        // Act
        var resultado = await _notaFiscalService.EmitirAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.Status); // Autorizada
        Assert.Equal("35260401123456789012345678901234567890123", resultado.ChaveAcesso);
        Assert.Equal("2026040100012345", resultado.ProtocoloAutorizacao);
        Assert.Equal("Autorizada", resultado.StatusNome);

        _notaFiscalRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<NotaFiscal>()), Times.Once);
        _sefazServiceMock.Verify(s => s.AutorizarNotaAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task EmitirAsync_ComVendaNaoExistente_DeveLancarExcecao()
    {
        // Arrange
        var vendaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var request = new RequestEmitirNotaFiscalJson
        {
            VendasId = vendaId,
            ClienteId = clienteId,
            Serie = 1,
            Numero = 100
        };

        _vendasRepositoryMock.Setup(r => r.GetByIdAsync(vendaId))
            .ReturnsAsync((Vendas?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _notaFiscalService.EmitirAsync(request));
    }

    [Fact]
    public async Task ConsultarStatusAsync_ComChaveValida_DeveRetornarStatus()
    {
        // Arrange
        var notaId = Guid.NewGuid();
        var notaFiscal = new NotaFiscal(Guid.NewGuid(), 1, 100);
        notaFiscal.AtualizarChaveAcesso("35260401123456789012345678901234567890123");
        notaFiscal.MarcarComoPendente();

        _notaFiscalRepositoryMock.Setup(r => r.ObterPorIdAsync(notaId))
            .ReturnsAsync(notaFiscal);

        _sefazServiceMock.Setup(s => s.ConsultarStatusAsync("35260401123456789012345678901234567890123"))
            .ReturnsAsync(new SefazConsultaResponse
            {
                Sucesso = true,
                Status = "100",
                Protocolo = "2026040100012345",
                Mensagem = "Autorizado"
            });

        _notaFiscalRepositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<NotaFiscal>()))
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(m => m.Map<ResponseNotaFiscalJson>(It.IsAny<NotaFiscal>()))
            .Returns(new ResponseNotaFiscalJson
            {
                Id = notaId,
                Status = 3,
                StatusNome = "Autorizada"
            });

        // Act
        var resultado = await _notaFiscalService.ConsultarStatusAsync(notaId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.Status); // Autorizada

        _sefazServiceMock.Verify(s => s.ConsultarStatusAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CancelarAsync_ComChaveValida_DeveRetornarNotaCancelada()
    {
        // Arrange
        var notaId = Guid.NewGuid();
        var notaFiscal = new NotaFiscal(Guid.NewGuid(), 1, 100);
        notaFiscal.AtualizarChaveAcesso("35260401123456789012345678901234567890123");
        notaFiscal.MarcarComoAutorizada("2026040100012345", "<xml>autorizado</xml>");

        var request = new RequestCancelarNotaFiscalJson
        {
            Justificativa = "Produto devolvido pelo cliente. Motivo: defeito na embalagem."
        };

        _notaFiscalRepositoryMock.Setup(r => r.ObterPorIdAsync(notaId))
            .ReturnsAsync(notaFiscal);

        _sefazServiceMock.Setup(s => s.CancelarNotaAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SefazCancelamentoResponse
            {
                Sucesso = true,
                NsuCancelamento = "000000001",
                Mensagem = "Cancelamento registrado com sucesso"
            });

        _notaFiscalRepositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<NotaFiscal>()))
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(m => m.Map<ResponseNotaFiscalJson>(It.IsAny<NotaFiscal>()))
            .Returns(new ResponseNotaFiscalJson
            {
                Id = notaId,
                Status = 5,
                StatusNome = "Cancelada"
            });

        // Act
        var resultado = await _notaFiscalService.CancelarAsync(notaId, request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(5, resultado.Status); // Cancelada

        _sefazServiceMock.Verify(s => s.CancelarNotaAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
