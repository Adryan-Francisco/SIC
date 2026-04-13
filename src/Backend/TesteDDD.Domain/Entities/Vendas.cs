namespace TesteDDD.Domain.Entities;

public class Vendas
{
    public Guid Id { get; private set; }
    public DateTime DataVenda { get; private set; }
    public decimal ValorTotal { get; private set; }
    public List<ItemVendas> Itens { get; private set; } = new();

    protected Vendas()
    {
    }

    public Vendas(DateTime dataVenda, List<ItemVendas> itens)
    {
        Id = Guid.NewGuid();
        Update(dataVenda, itens);
    }

    public void Update(DateTime dataVenda, List<ItemVendas> itens)
    {
        if (dataVenda == default)
            throw new ArgumentException("Data da venda e obrigatoria.", nameof(dataVenda));
        if (itens == null || itens.Count == 0)
            throw new ArgumentException("A venda deve conter pelo menos um item.", nameof(itens));

        DataVenda = dataVenda;
        Itens = itens;
        ValorTotal = itens.Sum(item => item.CalcularValorTotal());
    }
}
