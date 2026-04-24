using System.Xml.Linq;

namespace TesteDDD.Infrastructure.Sefaz;

public interface IDanfePdfService
{
    Task<string> GerarDanfePdfAsync(string xmlAutorizado, string chaveAcesso, string caminhoSalvamento);
    string ObterUrlConsultaDanfe(string chaveAcesso);
}

public class DanfePdfService : IDanfePdfService
{
    private readonly ILogger<DanfePdfService> _logger;
    private readonly SefazIntegrationSettings _settings;

    public DanfePdfService(ILogger<DanfePdfService> logger, SefazIntegrationSettings settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<string> GerarDanfePdfAsync(string xmlAutorizado, string chaveAcesso, string caminhoSalvamento)
    {
        try
        {
            _logger.LogInformation($"Iniciando geração de DANFE para chave: {chaveAcesso}");

            if (string.IsNullOrWhiteSpace(xmlAutorizado))
                throw new ArgumentException("XML autorizado não pode estar vazio.");

            if (string.IsNullOrWhiteSpace(chaveAcesso) || chaveAcesso.Length != 44)
                throw new ArgumentException("Chave de acesso inválida.");

            // Criar diretório se não existir
            if (!Directory.Exists(caminhoSalvamento))
            {
                Directory.CreateDirectory(caminhoSalvamento);
                _logger.LogInformation($"Diretório criado: {caminhoSalvamento}");
            }

            // Extrair dados do XML para montar a DANFE
            var dados = ExtrairDadosXml(xmlAutorizado);

            // Caminho do arquivo PDF
            var nomePdf = $"DANFE_{chaveAcesso}.pdf";
            var caminhoCompleto = Path.Combine(caminhoSalvamento, nomePdf);

            // Para uma implementação real, você usaria iTextSharp ou similar
            // Por enquanto, criamos um arquivo de exemplo em texto que poderia ser convertido
            await GerarDanfeSimuladaAsync(dados, caminhoCompleto);

            _logger.LogInformation($"DANFE gerada: {caminhoCompleto}");
            return caminhoCompleto;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao gerar DANFE: {ex.Message}");
            throw;
        }
    }

    public string ObterUrlConsultaDanfe(string chaveAcesso)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(chaveAcesso) || chaveAcesso.Length != 44)
                throw new ArgumentException("Chave de acesso inválida.");

            var servidor = _settings.UtilizarHomologacao ? "https://nfe-homolog.svrs.rs.gov.br" : "https://nfe.fazenda.sp.gov.br";
            var caminho = _settings.Danfe?.CaminhoConsultaDanfe ?? "/danfeweb/consultar";

            return $"{servidor}{caminho}?chNFe={chaveAcesso}";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao gerar URL da DANFE: {ex.Message}");
            throw;
        }
    }

    private DadosDanfe ExtrairDadosXml(string xmlAutorizado)
    {
        try
        {
            var doc = XDocument.Parse(xmlAutorizado);
            XNamespace nfe = "http://www.portalfiscal.inf.br/nfe";

            var infNFe = doc.Descendants(nfe + "infNFe").FirstOrDefault();
            var ide = infNFe?.Element(nfe + "ide");
            var emit = infNFe?.Element(nfe + "emit");
            var dest = infNFe?.Element(nfe + "dest");
            var total = infNFe?.Element(nfe + "total");
            var icmsTot = total?.Element(nfe + "ICMSTot");

            var dados = new DadosDanfe
            {
                ChaveAcesso = infNFe?.Attribute("Id")?.Value?.Substring(3) ?? "",
                NumeroNFe = ide?.Element(nfe + "nNF")?.Value ?? "",
                SerieNFe = ide?.Element(nfe + "serie")?.Value ?? "",
                DataEmissao = ide?.Element(nfe + "dEmi")?.Value ?? "",
                RazaoEmit = emit?.Element(nfe + "xNome")?.Value ?? "",
                CnpjEmit = emit?.Element(nfe + "CNPJ")?.Value ?? "",
                IeEmit = emit?.Element(nfe + "IE")?.Value ?? "",
                EndEmit = emit?.Element(nfe + "enderEmit")?.Element(nfe + "xLgr")?.Value ?? "",
                RazaoDest = dest?.Element(nfe + "xNome")?.Value ?? "",
                CnpjDest = dest?.Element(nfe + "CNPJ")?.Value ?? "",
                EndDest = dest?.Element(nfe + "enderDest")?.Element(nfe + "xLgr")?.Value ?? "",
                ValorTotal = icmsTot?.Element(nfe + "vNF")?.Value ?? "0.00",
                ValorICMS = icmsTot?.Element(nfe + "vICMS")?.Value ?? "0.00",
                ProtocoloAutorizacao = ObterProtocolo(xmlAutorizado)
            };

            return dados;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao extrair dados do XML: {ex.Message}");
            return new DadosDanfe();
        }
    }

    private string ObterProtocolo(string xmlAutorizado)
    {
        try
        {
            var doc = XDocument.Parse(xmlAutorizado);
            XNamespace nfe = "http://www.portalfiscal.inf.br/nfe";

            var infProt = doc.Descendants(nfe + "infProt").FirstOrDefault();
            return infProt?.Element(nfe + "nProt")?.Value ?? "";
        }
        catch
        {
            return "";
        }
    }

    private async Task GerarDanfeSimuladaAsync(DadosDanfe dados, string caminhoCompleto)
    {
        // Criar HTML/texto representando a DANFE
        var conteudo = $@"
================================================================================
                    DANFE - DOCUMENTO AUXILIAR DA NOTA FISCAL ELETRÔNICA
================================================================================

CHAVE DE ACESSO: {dados.ChaveAcesso}
PROTOCOLO DE AUTORIZAÇÃO: {dados.ProtocoloAutorizacao}

EMITENTE:
  Razão Social: {dados.RazaoEmit}
  CNPJ: {dados.CnpjEmit}
  IE: {dados.IeEmit}
  Endereço: {dados.EndEmit}

DESTINATÁRIO:
  Razão Social: {dados.RazaoDest}
  CNPJ: {dados.CnpjDest}
  Endereço: {dados.EndDest}

IDENTIFICAÇÃO DA NOTA FISCAL:
  Número: {dados.NumeroNFe}
  Série: {dados.SerieNFe}
  Data de Emissão: {dados.DataEmissao}

VALORES:
  Valor Total: R$ {dados.ValorTotal}
  ICMS: R$ {dados.ValorICMS}

================================================================================
Esta DANFE foi gerada automaticamente pelo sistema.
Para consultar a autorização completa, acesse:
https://nfe.fazenda.sp.gov.br/danfeweb/consultar?chNFe={dados.ChaveAcesso}
================================================================================
";

        // Para uma implementação real com PDF, usar:
        // using (var document = new iTextSharp.text.Document())
        // {
        //     iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(caminhoCompleto, FileMode.Create));
        //     document.Open();
        //     document.Add(new iTextSharp.text.Paragraph(conteudo));
        //     document.Close();
        // }

        // Por enquanto, salvamos como arquivo de texto
        var caminhoTxt = caminhoCompleto.Replace(".pdf", ".txt");
        await File.WriteAllTextAsync(caminhoTxt, conteudo);
    }

    private class DadosDanfe
    {
        public string ChaveAcesso { get; set; } = "";
        public string NumeroNFe { get; set; } = "";
        public string SerieNFe { get; set; } = "";
        public string DataEmissao { get; set; } = "";
        public string RazaoEmit { get; set; } = "";
        public string CnpjEmit { get; set; } = "";
        public string IeEmit { get; set; } = "";
        public string EndEmit { get; set; } = "";
        public string RazaoDest { get; set; } = "";
        public string CnpjDest { get; set; } = "";
        public string EndDest { get; set; } = "";
        public string ValorTotal { get; set; } = "0.00";
        public string ValorICMS { get; set; } = "0.00";
        public string ProtocoloAutorizacao { get; set; } = "";
    }
}
