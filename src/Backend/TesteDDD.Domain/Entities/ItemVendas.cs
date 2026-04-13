namespace TesteDDD.Domain.Entities;

public class ItemVendas
{
    public Guid Id { get; private set; }
    public Guid ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }

    protected ItemVendas()
    {
    }

    public ItemVendas(Guid produtoId, int quantidade, decimal precoUnitario)
    {
        Id = Guid.NewGuid();
        ProdutoId = produtoId;
        Update(quantidade, precoUnitario);
    }

    public void DefinirProduto(Produto produto)
    {
        Produto = produto ?? throw new ArgumentNullException(nameof(produto));
        ProdutoId = produto.Id;
    }

    public void Update(int quantidade, decimal precoUnitario)
    {
        if (ProdutoId == Guid.Empty)
            throw new ArgumentException("ProdutoId e obrigatorio.", nameof(ProdutoId));
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
        if (precoUnitario <= 0)
            throw new ArgumentException("Preco unitario deve ser maior que zero.", nameof(precoUnitario));

        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }

    public decimal CalcularValorTotal()
    {
        return Quantidade * PrecoUnitario;
    }
}
