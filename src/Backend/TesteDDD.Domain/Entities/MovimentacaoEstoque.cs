namespace TesteDDD.Domain.Entities;

public class MovimentacaoEstoque
{
    public Guid Id { get; private set; }
    public Guid ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public TipoMovimentacaoEstoque Tipo { get; private set; }
    public int Quantidade { get; private set; }
    public int QuantidadeAnterior { get; private set; }
    public int QuantidadeAtual { get; private set; }
    public string Observacao { get; private set; } = string.Empty;
    public DateTime DataMovimentacao { get; private set; }

    protected MovimentacaoEstoque()
    {
    }

    public MovimentacaoEstoque(Guid produtoId, TipoMovimentacaoEstoque tipo, int quantidade, int quantidadeAnterior, int quantidadeAtual, string observacao)
    {
        if (produtoId == Guid.Empty)
            throw new ArgumentException("ProdutoId e obrigatorio.", nameof(produtoId));
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade da movimentacao deve ser maior que zero.", nameof(quantidade));

        Id = Guid.NewGuid();
        ProdutoId = produtoId;
        Tipo = tipo;
        Quantidade = quantidade;
        QuantidadeAnterior = quantidadeAnterior;
        QuantidadeAtual = quantidadeAtual;
        Observacao = observacao?.Trim() ?? string.Empty;
        DataMovimentacao = DateTime.UtcNow;
    }
}

public enum TipoMovimentacaoEstoque
{
    Entrada = 1,
    Saida = 2,
    Ajuste = 3
}
