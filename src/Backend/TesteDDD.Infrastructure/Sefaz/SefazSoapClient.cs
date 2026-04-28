using Microsoft.Extensions.Logging;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Text;

namespace TesteDDD.Infrastructure.Sefaz;

public interface ISefazSoapClient
{
    Task<SefazAutorizacaoResponse> AutorizarNfeAsync(string xmlAssinado, string cnpjCert);
    Task<SefazConsultaResponse> ConsultarStatusNfeAsync(string chaveAcesso, string cnpjCert);
    Task<SefazCancelamentoResponse> CancelarNfeAsync(string xml, string cnpjCert);
}

public class SefazSoapClient : ISefazSoapClient
{
    private readonly SefazIntegrationSettings _settings;
    private readonly ILogger<SefazSoapClient> _logger;
    private readonly HttpClient _httpClient;

    private const string NamespaceNfe = "http://www.portalfiscal.inf.br/nfe";
    private const string NamespaceSoap = "http://schemas.xmlsoap.org/soap/envelope/";

    public SefazSoapClient(
        SefazIntegrationSettings settings,
        ILogger<SefazSoapClient> logger,
        HttpClient httpClient)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<SefazAutorizacaoResponse> AutorizarNfeAsync(string xmlAssinado, string cnpjCert)
    {
        try
        {
            _logger.LogInformation("Iniciando autorização de NFe via SOAP Sefaz");

            if (string.IsNullOrWhiteSpace(xmlAssinado))
                throw new ArgumentException("XML assinado não pode estar vazio.");

            var url = ObterUrlAutorizacao();
            
            // Criar envelope SOAP com o XML da NFe
            var soapEnvelope = CriarEnvelopeSoapAutorizacao(xmlAssinado);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml")
            };

            // Adicionar headers SOAP obrigatórios
            request.Headers.Add("SOAPAction", "");
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Connection", "Keep-Alive");

            var timeout = TimeSpan.FromSeconds(_settings.TimeoutSegundos ?? 30);
            var cts = new System.Threading.CancellationTokenSource(timeout);

            var response = await _httpClient.SendAsync(request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Erro na autorização: HTTP {response.StatusCode}");
                return new SefazAutorizacaoResponse
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao conectar com Sefaz: {response.StatusCode}"
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            return ProcessarRespostaAutorizacao(content);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Erro de conexão com Sefaz: {ex.Message}");
            return new SefazAutorizacaoResponse
            {
                Sucesso = false,
                Mensagem = $"Erro de conectividade: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao autorizar NFe: {ex.Message}");
            return new SefazAutorizacaoResponse
            {
                Sucesso = false,
                Mensagem = $"Erro ao autorizar: {ex.Message}"
            };
        }
    }

    public async Task<SefazConsultaResponse> ConsultarStatusNfeAsync(string chaveAcesso, string cnpjCert)
    {
        try
        {
            _logger.LogInformation($"Consultando status da NFe: {chaveAcesso}");

            if (string.IsNullOrWhiteSpace(chaveAcesso) || chaveAcesso.Length != 44)
                throw new ArgumentException("Chave de acesso inválida.");

            var url = ObterUrlConsulta();
            var soapEnvelope = CriarEnvelopeSoapConsulta(chaveAcesso);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml")
            };

            request.Headers.Add("SOAPAction", "");

            var timeout = TimeSpan.FromSeconds(_settings.TimeoutSegundos ?? 30);
            var cts = new System.Threading.CancellationTokenSource(timeout);

            var response = await _httpClient.SendAsync(request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Erro na consulta: HTTP {response.StatusCode}");
                return new SefazConsultaResponse
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao conectar com Sefaz: {response.StatusCode}"
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            return ProcessarRespostaConsulta(content);
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

    public async Task<SefazCancelamentoResponse> CancelarNfeAsync(string xml, string cnpjCert)
    {
        try
        {
            _logger.LogInformation("Cancelando NFe via SOAP Sefaz");

            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentException("XML de cancelamento não pode estar vazio.");

            var url = ObterUrlCancelamento();
            var soapEnvelope = CriarEnvelopeSoapCancelamento(xml);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml")
            };

            request.Headers.Add("SOAPAction", "");

            var timeout = TimeSpan.FromSeconds(_settings.TimeoutSegundos ?? 30);
            var cts = new System.Threading.CancellationTokenSource(timeout);

            var response = await _httpClient.SendAsync(request, cts.Token);

            var content = await response.Content.ReadAsStringAsync();
            return ProcessarRespostaCancelamento(content);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao cancelar NFe: {ex.Message}");
            return new SefazCancelamentoResponse
            {
                Sucesso = false,
                Mensagem = $"Erro ao cancelar: {ex.Message}"
            };
        }
    }

    private string ObterUrlAutorizacao()
    {
        var baseUrl = _settings.UtilizarHomologacao ? _settings.UrlHomologacao : _settings.UrlProducao;
        return $"{baseUrl}/NfeAutorizacao4?wsdl";
    }

    private string ObterUrlConsulta()
    {
        var baseUrl = _settings.UtilizarHomologacao ? _settings.UrlHomologacao : _settings.UrlProducao;
        return $"{baseUrl}/NfeRetAutorizacao4?wsdl";
    }

    private string ObterUrlCancelamento()
    {
        var baseUrl = _settings.UtilizarHomologacao ? _settings.UrlHomologacao : _settings.UrlProducao;
        return $"{baseUrl}/NfeCancelamento4?wsdl";
    }

    private string CriarEnvelopeSoapAutorizacao(string xmlNfe)
    {
        var nfeCompactada = CompactarXml(xmlNfe);

        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:nfe=""http://www.portalfiscal.inf.br/nfe"">
    <soap:Header />
    <soap:Body>
        <nfe:nfeDadosMsg>{nfeCompactada}</nfe:nfeDadosMsg>
    </soap:Body>
</soap:Envelope>";
    }

    private string CriarEnvelopeSoapConsulta(string chaveAcesso)
    {
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:nfe=""http://www.portalfiscal.inf.br/nfe"">
    <soap:Header />
    <soap:Body>
        <nfe:nfeResultMsg>
            <chNFe>{chaveAcesso}</chNFe>
        </nfe:nfeResultMsg>
    </soap:Body>
</soap:Envelope>";
    }

    private string CriarEnvelopeSoapCancelamento(string xmlCancelamento)
    {
        var xmlCompactado = CompactarXml(xmlCancelamento);

        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:nfe=""http://www.portalfiscal.inf.br/nfe"">
    <soap:Header />
    <soap:Body>
        <nfe:nfeDadosMsg>{xmlCompactado}</nfe:nfeDadosMsg>
    </soap:Body>
</soap:Envelope>";
    }

    private string CompactarXml(string xml)
    {
        try
        {
            // Remove espaços em branco desnecessários para reduzir tamanho
            var doc = XDocument.Parse(xml);
            return doc.ToString(SaveOptions.DisableFormatting);
        }
        catch
        {
            return xml;
        }
    }

    private SefazAutorizacaoResponse ProcessarRespostaAutorizacao(string xmlResposta)
    {
        try
        {
            var doc = XDocument.Parse(xmlResposta);
            
            // Namespace SOAP
            XNamespace soap = NamespaceSoap;
            XNamespace nfe = NamespaceNfe;

            var body = doc.Descendants(soap + "Body").FirstOrDefault();
            if (body == null)
            {
                _logger.LogWarning("Body SOAP não encontrado na resposta");
                return new SefazAutorizacaoResponse { Sucesso = false, Mensagem = "Resposta inválida do Sefaz" };
            }

            // Verificar se houve fault (erro)
            var fault = body.Descendants(soap + "Fault").FirstOrDefault();
            if (fault != null)
            {
                var faultString = fault.Element(soap + "faultstring")?.Value ?? "Erro desconhecido";
                _logger.LogError($"Fault SOAP: {faultString}");
                return new SefazAutorizacaoResponse { Sucesso = false, Mensagem = faultString };
            }

            // Extrair dados da resposta
            var protNFe = body.Descendants(nfe + "protNFe").FirstOrDefault();
            if (protNFe == null)
            {
                _logger.LogWarning("Protocolo não encontrado na resposta");
                return new SefazAutorizacaoResponse { Sucesso = false, Mensagem = "Protocolo não recebido" };
            }

            var infProt = protNFe.Element(nfe + "infProt");
            var status = infProt?.Element(nfe + "cStat")?.Value ?? "";
            var protocolo = infProt?.Element(nfe + "nProt")?.Value ?? "";
            var xMotivo = infProt?.Element(nfe + "xMotivo")?.Value ?? "";

            // Status 100 = Autorizado, 110 = Autorizado com pendência
            var autorizado = status == "100" || status == "110";

            if (autorizado)
            {
                _logger.LogInformation($"NFe autorizada com protocolo: {protocolo}");
                return new SefazAutorizacaoResponse
                {
                    Sucesso = true,
                    Protocolo = protocolo,
                    XmlAutorizado = xmlResposta,
                    ChaveAcesso = ExtrairChaveAcesso(body),
                    Mensagem = xMotivo
                };
            }
            else
            {
                _logger.LogWarning($"NFe rejeitada: {xMotivo}");
                return new SefazAutorizacaoResponse
                {
                    Sucesso = false,
                    Mensagem = $"Rejeitada: {xMotivo}",
                    Protocolo = protocolo
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao processar resposta de autorização: {ex.Message}");
            return new SefazAutorizacaoResponse { Sucesso = false, Mensagem = $"Erro ao processar resposta: {ex.Message}" };
        }
    }

    private SefazConsultaResponse ProcessarRespostaConsulta(string xmlResposta)
    {
        try
        {
            var doc = XDocument.Parse(xmlResposta);
            XNamespace soap = NamespaceSoap;
            XNamespace nfe = NamespaceNfe;

            var body = doc.Descendants(soap + "Body").FirstOrDefault();
            if (body == null)
                return new SefazConsultaResponse { Sucesso = false, Mensagem = "Resposta inválida" };

            var fault = body.Descendants(soap + "Fault").FirstOrDefault();
            if (fault != null)
            {
                var faultString = fault.Element(soap + "faultstring")?.Value ?? "Erro";
                return new SefazConsultaResponse { Sucesso = false, Mensagem = faultString };
            }

            var protNFe = body.Descendants(nfe + "protNFe").FirstOrDefault();
            var infProt = protNFe?.Element(nfe + "infProt");
            var status = infProt?.Element(nfe + "cStat")?.Value ?? "";
            var protocolo = infProt?.Element(nfe + "nProt")?.Value ?? "";
            var xMotivo = infProt?.Element(nfe + "xMotivo")?.Value ?? "";

            _logger.LogInformation($"Status consultado: {status} - {xMotivo}");

            return new SefazConsultaResponse
            {
                Sucesso = true,
                Status = status,
                Protocolo = protocolo,
                Mensagem = xMotivo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao processar resposta de consulta: {ex.Message}");
            return new SefazConsultaResponse { Sucesso = false, Mensagem = $"Erro: {ex.Message}" };
        }
    }

    private SefazCancelamentoResponse ProcessarRespostaCancelamento(string xmlResposta)
    {
        try
        {
            var doc = XDocument.Parse(xmlResposta);
            XNamespace soap = NamespaceSoap;
            XNamespace nfe = NamespaceNfe;

            var body = doc.Descendants(soap + "Body").FirstOrDefault();
            if (body == null)
                return new SefazCancelamentoResponse { Sucesso = false, Mensagem = "Resposta inválida" };

            var fault = body.Descendants(soap + "Fault").FirstOrDefault();
            if (fault != null)
            {
                var faultString = fault.Element(soap + "faultstring")?.Value ?? "Erro";
                return new SefazCancelamentoResponse { Sucesso = false, Mensagem = faultString };
            }

            var retEvento = body.Descendants(nfe + "retEvento").FirstOrDefault();
            var infEvento = retEvento?.Element(nfe + "infEvento");
            var status = infEvento?.Element(nfe + "cStat")?.Value ?? "";
            var nsu = infEvento?.Element(nfe + "nSeqEvento")?.Value ?? "";
            var xMotivo = infEvento?.Element(nfe + "xMotivo")?.Value ?? "";

            var sucesso = status == "128"; // 128 = Evento registrado

            _logger.LogInformation($"Cancelamento: Status {status} - {xMotivo}");

            return new SefazCancelamentoResponse
            {
                Sucesso = sucesso,
                NsuCancelamento = nsu,
                Mensagem = xMotivo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao processar resposta de cancelamento: {ex.Message}");
            return new SefazCancelamentoResponse { Sucesso = false, Mensagem = $"Erro: {ex.Message}" };
        }
    }

    private string ExtrairChaveAcesso(XElement body)
    {
        try
        {
            XNamespace nfe = NamespaceNfe;
            var infNFe = body.Descendants(nfe + "infNFe").FirstOrDefault();
            var id = infNFe?.Attribute("Id")?.Value ?? "";
            return id.Length > 3 ? id.Substring(3) : "";
        }
        catch
        {
            return "";
        }
    }
}
