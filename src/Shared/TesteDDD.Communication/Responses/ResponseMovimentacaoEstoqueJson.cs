namespace TesteDDD.Communication.Responses;

public class ResponseMovimentacaoEstoqueJson
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int Tipo { get; set; }
    public string TipoNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int QuantidadeAnterior { get; set; }
    public int QuantidadeAtual { get; set; }
    public string Observacao { get; set; } = string.Empty;
    public DateTime DataMovimentacao { get; set; }
}
