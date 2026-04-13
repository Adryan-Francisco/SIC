namespace TesteDDD.Communication.Responses;

public class ResponseVendasJson
{
    public Guid Id { get; set; }
    public DateTime DataVenda { get; set; }
    public decimal Total { get; set; }
    public List<ResponseItemVendasJson> Itens { get; set; } = new();
}
