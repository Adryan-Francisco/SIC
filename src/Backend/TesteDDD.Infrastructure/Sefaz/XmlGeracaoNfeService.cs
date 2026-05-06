using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using TesteDDD.Domain.Entities;

namespace TesteDDD.Infrastructure.Sefaz;

public interface IXmlGeracaoNfeService
{
    string GerarXmlNFe(NotaFiscal notaFiscal, Vendas venda, Cliente cliente, List<ItemVendas> itens, string cnpjEmitente);
    string CalcularChaveAcesso(string uf, DateTime dataEmissao, string cnpjEmitente, int serie, int numero);
}

public class XmlGeracaoNfeService : IXmlGeracaoNfeService
{
    private readonly ILogger<XmlGeracaoNfeService> _logger;
    private const string VersaoNfe = "4.00";
    private const string TpAmb = "2"; // 1=Produção, 2=Homologação

    public XmlGeracaoNfeService(ILogger<XmlGeracaoNfeService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GerarXmlNFe(NotaFiscal notaFiscal, Vendas venda, Cliente cliente, List<ItemVendas> itens, string cnpjEmitente)
    {
        try
        {
            _logger.LogInformation("Gerando XML NFe para venda: {VendasId}", venda.Id);

            if (venda == null) throw new ArgumentNullException(nameof(venda));
            if (cliente == null) throw new ArgumentNullException(nameof(cliente));
            if (itens == null || itens.Count == 0) throw new ArgumentException("Itens não podem estar vazios.");

            var uf = "SP"; // TODO: Tornar configurável
            var cMun = "3550308"; // São Paulo - TODO: Tornar configurável baseado em UF
            var cnpjLimpo = LimparCnpj(cnpjEmitente);
            var chaveAcesso = CalcularChaveAcesso(uf, notaFiscal.DataEmissao, cnpjLimpo, notaFiscal.Serie, notaFiscal.Numero);

            var nfeElement = new XElement("NFe",
                new XAttribute("xmlns", "http://www.portalfiscal.inf.br/nfe"),
                GerarInfNFe(notaFiscal, venda, cliente, itens, cnpjLimpo, uf, cMun, chaveAcesso)
            );

            var xmlDoc = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                nfeElement
            );

            var xml = xmlDoc.ToString();
            _logger.LogInformation("XML NFe gerado com sucesso. Chave: {ChaveAcesso}", chaveAcesso);
            return xml;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao gerar XML NFe: {ex.Message}");
            throw;
        }
    }

    private XElement GerarInfNFe(NotaFiscal notaFiscal, Vendas venda, Cliente cliente, List<ItemVendas> itens, 
        string cnpjEmitente, string uf, string cMun, string chaveAcesso)
    {
        var id = $"NFe{chaveAcesso}";

        return new XElement("infNFe",
            new XAttribute("Id", id),
            new XAttribute("versao", VersaoNfe),
            GerarIde(notaFiscal, cMun, chaveAcesso),
            GerarEmit(cnpjEmitente),
            GerarDest(cliente),
            GerarDet(itens),
            GerarTotal(venda),
            GerarTransp(),
            GerarCobr(),
            GerarPag(),
            GerarInfAdic()
        );
    }

    private XElement GerarIde(NotaFiscal notaFiscal, string cMun, string chaveAcesso)
    {
        var dataEmissao = notaFiscal.DataEmissao.ToString("yyyy-MM-dd");
        var horaEmissao = notaFiscal.DataEmissao.ToString("HH:mm:ss");

        return new XElement("ide",
            new XElement("cUF", "35"), // São Paulo
            new XElement("cNF", chaveAcesso.Substring(0, 8)),
            new XElement("natOp", "Venda de mercadoria"),
            new XElement("indPag", "0"), // 0=Pagamento à vista
            new XElement("mod", "55"), // Modelo 55 = NFe
            new XElement("serie", notaFiscal.Serie),
            new XElement("nNF", notaFiscal.Numero),
            new XElement("dEmi", dataEmissao),
            new XElement("hEmi", horaEmissao),
            new XElement("cDV", CalcularDigitoVerificador(chaveAcesso)),
            new XElement("tpNF", "1"), // 1=Saída
            new XElement("tpAmb", TpAmb), // 2=Homologação
            new XElement("tpEmis", "1"), // 1=Emissão normal
            new XElement("cMunFG", cMun),
            new XElement("finNFe", "1"), // 1=NF-e normal
            new XElement("indFinal", "0"), // 0=Não
            new XElement("indPres", "1"), // 1=Operação presencial
            new XElement("procEmi", "0"), // 0=Emissão de NF-e com aplicativo do contribuinte
            new XElement("verProc", "1.0")
        );
    }

    private XElement GerarEmit(string cnpjEmitente)
    {
        return new XElement("emit",
            new XElement("CNPJ", LimparCnpj(cnpjEmitente)),
            new XElement("xNome", "Empresa Teste LTDA"), // TODO: Tornar configurável
            new XElement("xFant", "Empresa Teste"), // TODO: Tornar configurável
            new XElement("enderEmit",
                new XElement("xLgr", "Rua Teste"),
                new XElement("nro", "123"),
                new XElement("xCpl", "Apto 101"),
                new XElement("xBairro", "Bairro Teste"),
                new XElement("cMun", "3550308"),
                new XElement("xMun", "São Paulo"),
                new XElement("UF", "SP"),
                new XElement("CEP", "01310100"),
                new XElement("cPais", "1058"),
                new XElement("xPais", "Brasil"),
                new XElement("fone", "1133334444")
            ),
            new XElement("IE", "123456789012345"), // TODO: Tornar configurável
            new XElement("IEST", "ISENTO"),
            new XElement("IM", "123456"),
            new XElement("CRT", "3") // 3=Simples Nacional
        );
    }

    private XElement GerarDest(Cliente cliente)
    {
        var cpfCnpj = new XElement("CPF", "12345678900"); // TODO: Obter do cliente

        return new XElement("dest",
            new XElement("CNPJ", "00000000000191"), // Cliente padrão quando não há info
            new XElement("xNome", cliente.Nome.Length > 60 ? cliente.Nome.Substring(0, 60) : cliente.Nome),
            new XElement("enderDest",
                new XElement("xLgr", cliente.Endereco.Length > 60 ? cliente.Endereco.Substring(0, 60) : cliente.Endereco),
                new XElement("nro", "0"),
                new XElement("xBairro", "Bairro"),
                new XElement("cMun", "3550308"),
                new XElement("xMun", "São Paulo"),
                new XElement("UF", "SP"),
                new XElement("CEP", cliente.Cep),
                new XElement("cPais", "1058"),
                new XElement("xPais", "Brasil")
            ),
            new XElement("indIEDest", "9") // 9=Isento
        );
    }

    private IEnumerable<XElement> GerarDet(List<ItemVendas> itens)
    {
        var detElements = new List<XElement>();
        int nItem = 1;

        foreach (var item in itens)
        {
            detElements.Add(new XElement("det",
                new XAttribute("nItem", nItem),
                new XElement("prod",
                    new XElement("CProd", item.ProdutoId.ToString("N").Substring(0, 16)), // Máximo 16 caracteres
                    new XElement("GTIN", ""),
                    new XElement("indTot", "1"),
                    new XElement("xProd", item.Produto?.Nome ?? "Produto"),
                    new XElement("NCM", "12345678"), // TODO: Obter do produto
                    new XElement("CEST", "123456"), // TODO: Tornar configurável
                    new XElement("CFOP", "5102"), // Venda de produção do estabelecimento
                    new XElement("u", "UN"),
                    new XElement("qCom", item.Quantidade.ToString("0.00")),
                    new XElement("vUnCom", item.PrecoUnitario.ToString("0.00")),
                    new XElement("vProd", item.CalcularValorTotal().ToString("0.00")),
                    new XElement("vDesc", "0.00"),
                    new XElement("vOutro", "0.00"),
                    new XElement("indTot", "1"),
                    new XElement("vTotTrib", "0.00")
                ),
                new XElement("imposto",
                    new XElement("ICMS",
                        new XElement("ICMS00",
                            new XElement("orig", "0"),
                            new XElement("CST", "00"),
                            new XElement("modBC", "0"),
                            new XElement("vBC", item.CalcularValorTotal().ToString("0.00")),
                            new XElement("pICMS", "7.00"),
                            new XElement("vICMS", (item.CalcularValorTotal() * 0.07m).ToString("0.00"))
                        )
                    ),
                    new XElement("IPI",
                        new XElement("cEnq", ""),
                        new XElement("IPITrib",
                            new XElement("CST", "50"),
                            new XElement("vBC", "0.00"),
                            new XElement("pIPI", "0.00"),
                            new XElement("vIPI", "0.00")
                        )
                    ),
                    new XElement("PIS",
                        new XElement("PISAliq",
                            new XElement("CST", "01"),
                            new XElement("vBC", item.CalcularValorTotal().ToString("0.00")),
                            new XElement("pPIS", "1.65"),
                            new XElement("vPIS", (item.CalcularValorTotal() * 0.0165m).ToString("0.00"))
                        )
                    ),
                    new XElement("COFINS",
                        new XElement("COFINSAliq",
                            new XElement("CST", "01"),
                            new XElement("vBC", item.CalcularValorTotal().ToString("0.00")),
                            new XElement("pCOFINS", "7.60"),
                            new XElement("vCOFINS", (item.CalcularValorTotal() * 0.076m).ToString("0.00"))
                        )
                    )
                ),
                new XElement("infAdProd", "")
            ));

            nItem++;
        }

        return detElements;
    }

    private XElement GerarTotal(Vendas venda)
    {
        var totalICMS = venda.ValorTotal * 0.07m; // 7%
        var totalPIS = venda.ValorTotal * 0.0165m; // 1.65%
        var totalCOFINS = venda.ValorTotal * 0.076m; // 7.6%

        return new XElement("total",
            new XElement("ICMSTot",
                new XElement("vBC", venda.ValorTotal.ToString("0.00")),
                new XElement("vICMS", totalICMS.ToString("0.00")),
                new XElement("vICMSDeson", "0.00"),
                new XElement("vFCP", "0.00"),
                new XElement("vBCST", "0.00"),
                new XElement("vST", "0.00"),
                new XElement("vFCPST", "0.00"),
                new XElement("vFCPSTRet", "0.00"),
                new XElement("vProd", venda.ValorTotal.ToString("0.00")),
                new XElement("vFrete", "0.00"),
                new XElement("vSeg", "0.00"),
                new XElement("vDesc", "0.00"),
                new XElement("vII", "0.00"),
                new XElement("vIPI", "0.00"),
                new XElement("vIPIDevol", "0.00"),
                new XElement("vPIS", totalPIS.ToString("0.00")),
                new XElement("vCOFINS", totalCOFINS.ToString("0.00")),
                new XElement("vOutro", "0.00"),
                new XElement("vNF", venda.ValorTotal.ToString("0.00"))
            )
        );
    }

    private XElement GerarTransp()
    {
        return new XElement("transp",
            new XElement("modFrete", "9") // 9=Sem ocorrência de transporte
        );
    }

    private XElement GerarCobr()
    {
        return new XElement("cobr",
            new XElement("dup",
                new XElement("nDup", "001"),
                new XElement("dVenc", DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd")),
                new XElement("vDup", "0.00")
            )
        );
    }

    private XElement GerarPag()
    {
        return new XElement("pag",
            new XElement("detPag",
                new XElement("tPag", "01"), // 01=Dinheiro
                new XElement("vPag", "0.00")
            )
        );
    }

    private XElement GerarInfAdic()
    {
        return new XElement("infAdic",
            new XElement("infCpl", "NFe emitida pelo sistema integrado")
        );
    }

    public string CalcularChaveAcesso(string uf, DateTime dataEmissao, string cnpjEmitente, int serie, int numero)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(uf) || uf.Length != 2)
                throw new ArgumentException("UF inválido.");

            cnpjEmitente = LimparCnpj(cnpjEmitente);
            if (cnpjEmitente.Length != 14)
                throw new ArgumentException("CNPJ inválido.");

            // Código da UF segundo IBGE
            var codigoUF = ObterCodigoUF(uf);
            var data = dataEmissao.ToString("yyMM");
            var cnpj = cnpjEmitente;
            var modelo = "55";
            var serieFormatado = serie.ToString("000");
            var numeroFormatado = numero.ToString("000000000");
            var nfe = "00000000"; // Número sequencial da NFe (normalmente 00000000)
            var tpEmis = "1"; // Tipo de emissão

            var chaveBase = $"{codigoUF}{data}{cnpj}{modelo}{serieFormatado}{numeroFormatado}{nfe}{tpEmis}";

            // Calcular dígito verificador
            var dv = CalcularDigitoVerificador(chaveBase);
            var chaveAcesso = chaveBase + dv;

            return chaveAcesso;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao calcular chave de acesso: {ex.Message}");
            throw;
        }
    }

    private string ObterCodigoUF(string uf)
    {
        return uf.ToUpper() switch
        {
            "AC" => "04",
            "AL" => "17",
            "AP" => "16",
            "AM" => "03",
            "BA" => "05",
            "CE" => "07",
            "DF" => "26",
            "ES" => "14",
            "GO" => "10",
            "MA" => "11",
            "MT" => "28",
            "MS" => "10",
            "MG" => "31",
            "PA" => "15",
            "PB" => "21",
            "PR" => "41",
            "PE" => "08",
            "PI" => "16",
            "RJ" => "33",
            "RN" => "24",
            "RS" => "43",
            "RO" => "23",
            "RR" => "24",
            "SC" => "24",
            "SP" => "35",
            "SE" => "28",
            "TO" => "29",
            _ => throw new InvalidOperationException($"UF não reconhecido: {uf}")
        };
    }

    public string CalcularDigitoVerificador(string sequencia)
    {
        if (string.IsNullOrWhiteSpace(sequencia))
            throw new ArgumentException("Sequência não pode estar vazia.");

        var multiplicadores = new[] { 2, 3, 4, 5, 6, 7, 8, 9 };
        var indice = 0;
        var soma = 0;

        for (int i = sequencia.Length - 1; i >= 0; i--)
        {
            soma += int.Parse(sequencia[i].ToString()) * multiplicadores[indice % multiplicadores.Length];
            indice++;
        }

        var resto = soma % 11;
        var digito = 11 - resto;

        return (digito >= 10 ? 0 : digito).ToString();
    }

    private string LimparCnpj(string cnpj)
    {
        return new string(cnpj.Where(char.IsDigit).ToArray());
    }
}
