namespace TesteDDD.Communication.Responses;

public class ResponseEstoqueJson
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int QuantidadeDisponivel { get; set; }
    public int QuantidadeMinima { get; set; }
}
