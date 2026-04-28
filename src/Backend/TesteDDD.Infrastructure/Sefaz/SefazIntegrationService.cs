using Microsoft.Extensions.Logging;
using System.Text;
using System.Xml.Linq;

namespace TesteDDD.Infrastructure.Sefaz;

public class SefazIntegrationSettings
{
    public string? UrlProducao { get; set; }
    public string? UrlHomologacao { get; set; }
    public bool UtilizarHomologacao { get; set; }
    public string? CertificadoCaminho { get; set; }
    public string? CertificadoSenha { get; set; }
    public string? CnpjEmitente { get; set; }
    public string? RazaoSocial { get; set; }
    public string? NomeFantasia { get; set; }
    public string? Uf { get; set; }
    public int? TimeoutSegundos { get; set; } = 30;
    public int? ConsultaStatusInterval { get; set; } = 2000;
    public int? ConsultaStatusMaxTentativas { get; set; } = 30;
    public DanfeSettings? Danfe { get; set; }
}

public class DanfeSettings
{
    public string? CaminhoSalvamento { get; set; }
    public string? Servidor { get; set; }
    public string? CaminhoConsultaDanfe { get; set; }
}

public interface ISefazIntegrationService
{
    Task<SefazAutorizacaoResponse> AutorizarNotaAsync(string xmlNota);
    Task<SefazConsultaResponse> ConsultarStatusAsync(string chaveAcesso);
    Task<SefazCancelamentoResponse> CancelarNotaAsync(string chaveAcesso, string justificativa);
}

public class SefazAutorizacaoResponse
{
    public bool Sucesso { get; set; }
    public string Protocolo { get; set; } = string.Empty;
    public string XmlAutorizado { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string ChaveAcesso { get; set; } = string.Empty;
}

public class SefazConsultaResponse
{
    public bool Sucesso { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Protocolo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
}

public class SefazCancelamentoResponse
{
    public bool Sucesso { get; set; }
    public string NsuCancelamento { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
}

public class SefazIntegrationService : ISefazIntegrationService
{
    private readonly SefazIntegrationSettings _settings;
    private readonly ILogger<SefazIntegrationService> _logger;
    private readonly IXmlSignatureService _xmlSignatureService;
    private readonly ISefazSoapClient _soapClient;
    private readonly IDanfePdfService _danfePdfService;

    public SefazIntegrationService(
        SefazIntegrationSettings settings,
        ILogger<SefazIntegrationService> logger,
        IXmlSignatureService xmlSignatureService,
        ISefazSoapClient soapClient,
        IDanfePdfService danfePdfService)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _xmlSignatureService = xmlSignatureService ?? throw new ArgumentNullException(nameof(xmlSignatureService));
        _soapClient = soapClient ?? throw new ArgumentNullException(nameof(soapClient));
        _danfePdfService = danfePdfService ?? throw new ArgumentNullException(nameof(danfePdfService));
    }

    public async Task<SefazAutorizacaoResponse> AutorizarNotaAsync(string xmlNota)
    {
        try
        {
            _logger.LogInformation("Iniciando processo de autorização de nota fiscal");

            // Validar XML básico
            if (string.IsNullOrWhiteSpace(xmlNota))
                throw new ArgumentException("XML da nota não pode estar vazio.", nameof(xmlNota));

            // Assinar XML com certificado digital
            var xmlAssinado = await _xmlSignatureService.AssinarXmlAsync(
                xmlNota,
                _settings.CertificadoCaminho ?? string.Empty,
                _settings.CertificadoSenha ?? string.Empty);

            _logger.LogInformation("XML assinado com sucesso, enviando para Sefaz");

            // Extrair CNPJ do certificado
            var certificado = _xmlSignatureService.CarregarCertificado(
                _settings.CertificadoCaminho ?? string.Empty,
                _settings.CertificadoSenha ?? string.Empty);

            var cnpjCert = certificado.Subject.Split(',')
                .FirstOrDefault(s => s.Contains("CN="))?
                .Replace("CN=", "").Trim() ?? string.Empty;

            // Enviar para Sefaz via SOAP
            var resposta = await _soapClient.AutorizarNfeAsync(xmlAssinado, cnpjCert);

            if (resposta.Sucesso)
            {
                // Gerar DANFE
                try
                {
                    var caminhoSalvamento = _settings.Danfe?.CaminhoSalvamento ?? "C:\\NFes\\DANFE";
                    var caminhoArquivo = await _danfePdfService.GerarDanfePdfAsync(
                        resposta.XmlAutorizado,
                        resposta.ChaveAcesso,
                        caminhoSalvamento);

                    // Gerar URL da DANFE
                    var urlDanfe = _danfePdfService.ObterUrlConsultaDanfe(resposta.ChaveAcesso);
                    resposta.Mensagem = $"Autorizada com sucesso. DANFE: {caminhoArquivo}";

                    _logger.LogInformation($"Nota autorizada e DANFE gerada: {caminhoArquivo}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Erro ao gerar DANFE: {ex.Message}");
                    // Continuar mesmo com erro na DANFE
                }
            }

            return resposta;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao autorizar nota: {ex.Message}");
            return new SefazAutorizacaoResponse
            {
                Sucesso = false,
                Mensagem = $"Erro ao autorizar: {ex.Message}"
            };
        }
    }

    public async Task<SefazConsultaResponse> ConsultarStatusAsync(string chaveAcesso)
    {
        try
        {
            _logger.LogInformation($"Consultando status da nota com chave: {chaveAcesso}");

            if (string.IsNullOrWhiteSpace(chaveAcesso) || chaveAcesso.Length != 44)
                throw new ArgumentException("Chave de acesso inválida.", nameof(chaveAcesso));

            // Extrair CNPJ do certificado
            var certificado = _xmlSignatureService.CarregarCertificado(
                _settings.CertificadoCaminho ?? string.Empty,
                _settings.CertificadoSenha ?? string.Empty);

            var cnpjCert = certificado.Subject.Split(',')
                .FirstOrDefault(s => s.Contains("CN="))?
                .Replace("CN=", "").Trim() ?? string.Empty;

            // Consultar no Sefaz
            var resposta = await _soapClient.ConsultarStatusNfeAsync(chaveAcesso, cnpjCert);

            _logger.LogInformation($"Status retornado: {resposta.Status}");
            return resposta;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao consultar status: {ex.Message}");
            return new SefazConsultaResponse
            {
                Sucesso = false,
                Mensagem = $"Erro ao consultar: {ex.Message}"
            };
        }
    }

    public async Task<SefazCancelamentoResponse> CancelarNotaAsync(string chaveAcesso, string justificativa)
    {
        try
        {
            _logger.LogInformation($"Cancelando nota com chave: {chaveAcesso}");

            if (string.IsNullOrWhiteSpace(chaveAcesso) || chaveAcesso.Length != 44)
                throw new ArgumentException("Chave de acesso inválida.", nameof(chaveAcesso));

            if (string.IsNullOrWhiteSpace(justificativa) || justificativa.Length < 15)
                throw new ArgumentException("Justificativa deve ter no mínimo 15 caracteres.", nameof(justificativa));

            // Gerar XML de cancelamento
            var xmlCancelamento = GerarXmlCancelamento(chaveAcesso, justificativa);

            // Assinar XML
            var xmlAssinado = await _xmlSignatureService.AssinarXmlAsync(
                xmlCancelamento,
                _settings.CertificadoCaminho ?? string.Empty,
                _settings.CertificadoSenha ?? string.Empty);

            // Extrair CNPJ do certificado
            var certificado = _xmlSignatureService.CarregarCertificado(
                _settings.CertificadoCaminho ?? string.Empty,
                _settings.CertificadoSenha ?? string.Empty);

            var cnpjCert = certificado.Subject.Split(',')
                .FirstOrDefault(s => s.Contains("CN="))?
                .Replace("CN=", "").Trim() ?? string.Empty;

            // Enviar para Sefaz
            var resposta = await _soapClient.CancelarNfeAsync(xmlAssinado, cnpjCert);

            _logger.LogInformation($"Cancelamento processado com NSU: {resposta.NsuCancelamento}");
            return resposta;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao cancelar nota: {ex.Message}");
            return new SefazCancelamentoResponse
            {
                Sucesso = false,
                Mensagem = $"Erro ao cancelar: {ex.Message}"
            };
        }
    }

    private string ExtrairChaveAcessoDoXml(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var infNFe = doc.Descendants(XName.Get("infNFe", "http://www.portalfiscal.inf.br/nfe")).FirstOrDefault();
            var idAttr = infNFe?.Attribute("Id");
            if (idAttr != null)
            {
                var id = idAttr.Value;
                return id.Length > 3 ? id.Substring(3) : string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Erro ao extrair chave de acesso: {ex.Message}");
        }
        return string.Empty;
    }

    private string GenerarProtocolo()
    {
        return DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Random.Shared.Next(100000, 999999);
    }

    private string GenerarNsu()
    {
        return Random.Shared.Next(100000000, 999999999).ToString();
    }

    private string GerarXmlCancelamento(string chaveAcesso, string justificativa)
    {
        var dataHora = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
        var sequencia = DateTime.UtcNow.Ticks.ToString().Substring(DateTime.UtcNow.Ticks.ToString().Length - 5);

        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<cancNFe xmlns=""http://www.portalfiscal.inf.br/nfe"" versaoCanc=""4.00"">
    <infCanc Id=""ID{sequencia}"" versao=""4.00"">
        <ide>
            <chNFe>{chaveAcesso}</chNFe>
            <dEmi>{dataHora.Substring(0, 10)}</dEmi>
            <hEmi>{dataHora.Substring(11)}</hEmi>
            <tpAmb>2</tpAmb>
            <CNPJ>{_settings.CnpjEmitente?.Replace(".", "").Replace("/", "").Replace("-", "")}</CNPJ>
            <signAC>SA</signAC>
            <assinaturaQRCode></assinaturaQRCode>
        </ide>
        <infEvento>
            <detEvento versaoEvento=""4.00"">
                <descEvento>Cancelamento</descEvento>
                <detCanc>
                    <descCancAlt>{justificativa}</descCancAlt>
                </detCanc>
            </detEvento>
        </infEvento>
    </infCanc>
</cancNFe>";
    }
}
