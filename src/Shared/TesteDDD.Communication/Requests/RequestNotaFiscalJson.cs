namespace TesteDDD.Communication.Requests;

public class RequestEmitirNotaFiscalJson
{
    public Guid VendasId { get; set; }
    public Guid ClienteId { get; set; }
    public int Serie { get; set; }
    public int Numero { get; set; }
}

public class RequestCancelarNotaFiscalJson
{
    public string Justificativa { get; set; } = string.Empty;
}
