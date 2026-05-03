using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace TesteDDD.Infrastructure.Sefaz;

public interface IXmlSignatureService
{
    Task<string> AssinarXmlAsync(string xml, string certificadoCaminho, string certificadoSenha);
    X509Certificate2 CarregarCertificado(string caminho, string senha);
}

public class XmlSignatureService : IXmlSignatureService
{
    private readonly ILogger<XmlSignatureService> _logger;

    public XmlSignatureService(ILogger<XmlSignatureService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<string> AssinarXmlAsync(string xml, string certificadoCaminho, string certificadoSenha)
    {
        try
        {
            _logger.LogInformation("Iniciando assinatura digital do XML");

            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentException("XML nao pode estar vazio.", nameof(xml));

            if (string.IsNullOrWhiteSpace(certificadoCaminho))
                throw new ArgumentException("Caminho do certificado nao pode estar vazio.", nameof(certificadoCaminho));

            var certificado = CarregarCertificado(certificadoCaminho, certificadoSenha);
            var privateKey = certificado.GetRSAPrivateKey()
                ?? throw new InvalidOperationException("Certificado nao contem chave privada RSA.");

            var xmlDoc = new XmlDocument
            {
                PreserveWhitespace = true
            };
            xmlDoc.LoadXml(xml);

            var infNFeElement = xmlDoc.GetElementsByTagName("infNFe")[0];
            if (infNFeElement == null)
                throw new InvalidOperationException("Elemento infNFe nao encontrado no XML.");

            var infNFeId = infNFeElement.Attributes?["Id"]?.Value;
            if (string.IsNullOrWhiteSpace(infNFeId))
                throw new InvalidOperationException("Atributo Id do elemento infNFe nao encontrado no XML.");

            var signedXml = new SignedXml(xmlDoc)
            {
                SigningKey = privateKey
            };

            var reference = new Reference { Uri = "#" + infNFeId };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigC14NTransform());
            signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificado));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();

            var signatureElement = signedXml.GetXml();
            var nfeElement = xmlDoc.GetElementsByTagName("NFe")[0];
            if (nfeElement != null)
                nfeElement.AppendChild(xmlDoc.ImportNode(signatureElement, true));

            _logger.LogInformation("XML assinado com sucesso");
            return Task.FromResult(xmlDoc.OuterXml);
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao assinar XML: {Message}", ex.Message);
            throw;
        }
    }

    public X509Certificate2 CarregarCertificado(string caminho, string senha)
    {
        try
        {
            _logger.LogInformation("Carregando certificado de: {Caminho}", caminho);

            if (!File.Exists(caminho))
                throw new FileNotFoundException($"Certificado nao encontrado: {caminho}");

            var certificado = new X509Certificate2(
                caminho,
                senha,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

            _logger.LogInformation("Certificado carregado: {Subject}", certificado.Subject);
            return certificado;
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao carregar certificado: {Message}", ex.Message);
            throw;
        }
    }
}
