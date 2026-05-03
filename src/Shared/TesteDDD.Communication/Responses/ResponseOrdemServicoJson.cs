namespace TesteDDD.Communication.Responses;

public class ResponseOrdemServicoJson
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public Guid ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataAbertura { get; set; }
    public DateTime? DataConclusao { get; set; }
    public decimal ValorServico { get; set; }
    public int Status { get; set; }
    public string StatusNome { get; set; } = string.Empty;
}
