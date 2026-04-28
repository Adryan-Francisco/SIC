namespace TesteDDD.Communication.Responses;

public class ResponseNotaFiscalJson
{
    public Guid Id { get; set; }
    public Guid VendasId { get; set; }
    public int Serie { get; set; }
    public int Numero { get; set; }
    public string ChaveAcesso { get; set; } = string.Empty;
    public DateTime DataEmissao { get; set; }
    public DateTime? DataAutorizacao { get; set; }
    public int Status { get; set; }
    public string StatusNome { get; set; } = string.Empty;
    public string ProtocoloAutorizacao { get; set; } = string.Empty;
    public string DanfeUrl { get; set; } = string.Empty;
    public string? MensagemErro { get; set; }
}
