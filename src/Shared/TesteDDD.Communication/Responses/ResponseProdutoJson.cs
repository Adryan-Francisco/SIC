namespace TesteDDD.Communication.Responses;

public class ResponseProdutoJson
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public Guid CategoriaId { get; set; }
    public Guid FornecedorId { get; set; }
    public string FornecedorNome { get; set; } = string.Empty;
}
