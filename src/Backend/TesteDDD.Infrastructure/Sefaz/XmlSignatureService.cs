using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;

namespace TesteDDD.Infrastructure.Sefaz;

public interface IXmlSignatureService
{
    Task<string> AssinarXmlAsync(string xml, string certificadoCaminho, string certificadoSenha);
    X509Certificate2 CarregarCertificado(string caminho, string senha);
}

public class XmlSignatureService : IXmlSignatureService
{
    private readonly ILogger<XmlSignatureService> _logger;
    private const string SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
    private const string DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
    private const string CanonicalMethod = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";

    public XmlSignatureService(ILogger<XmlSignatureService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> AssinarXmlAsync(string xml, string certificadoCaminho, string certificadoSenha)
    {
        try
        {
            _logger.LogInformation("Iniciando assinatura digital do XML");

            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentException("XML não pode estar vazio.", nameof(xml));

            if (string.IsNullOrWhiteSpace(certificadoCaminho))
                throw new ArgumentException("Caminho do certificado não pode estar vazio.", nameof(certificadoCaminho));

            // Carregar certificado
            var certificado = CarregarCertificado(certificadoCaminho, certificadoSenha);

            if (!certificado.HasPrivateKey)
                throw new InvalidOperationException("Certificado não contém chave privada.");

            // Carregar XML
            var xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(xml);

            // Encontrar o elemento infNFe para assinar
            var infNFeElement = xmlDoc.GetElementsByTagName("infNFe")[0];
            if (infNFeElement == null)
                throw new InvalidOperationException("Elemento infNFe não encontrado no XML.");

            // Criar objeto de assinatura
            var signedXml = new SignedXml(xmlDoc)
            {
                SigningKey = certificado.PrivateKey
            };

            // Configurar referência para o elemento infNFe
            var reference = new Reference { Uri = "#" + infNFeElement.Attributes["Id"]?.Value };

            // Adicionar transformações
            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            var c14n = new XmlDsigC14NTransform();
            reference.AddTransform(c14n);

            signedXml.AddReference(reference);

            // Configurar informações da chave
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificado));
            signedXml.KeyInfo = keyInfo;

            // Assinar
            signedXml.ComputeSignature();

            // Obter o XML de assinatura
            var signatureElement = signedXml.GetXml();

            // Inserir assinatura após infNFe
            var nfeElement = xmlDoc.GetElementsByTagName("NFe")[0];
            if (nfeElement != null)
            {
                nfeElement.AppendChild(xmlDoc.ImportNode(signatureElement, true));
            }

            _logger.LogInformation("XML assinado com sucesso");
            return xmlDoc.OuterXml;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao assinar XML: {ex.Message}");
            throw;
        }
    }

    public X509Certificate2 CarregarCertificado(string caminho, string senha)
    {
        try
        {
            _logger.LogInformation($"Carregando certificado de: {caminho}");

            if (!File.Exists(caminho))
                throw new FileNotFoundException($"Certificado não encontrado: {caminho}");

            var certificado = new X509Certificate2(caminho, senha, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

            _logger.LogInformation($"Certificado carregado: {certificado.Subject}");
            return certificado;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao carregar certificado: {ex.Message}");
            throw;
        }
    }
}
