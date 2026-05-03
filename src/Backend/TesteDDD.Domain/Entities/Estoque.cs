namespace TesteDDD.Domain.Entities;

public class Estoque
{
    public Guid Id { get; private set; }
    public Guid ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public int QuantidadeDisponivel { get; private set; }
    public int QuantidadeMinima { get; private set; }

    protected Estoque()
    {
    }

    public Estoque(Guid produtoId, int quantidadeDisponivel, int quantidadeMinima)
    {
        if (produtoId == Guid.Empty)
            throw new ArgumentException("ProdutoId e obrigatorio.", nameof(produtoId));

        Id = Guid.NewGuid();
        ProdutoId = produtoId;
        Configurar(quantidadeDisponivel, quantidadeMinima);
    }

    public void Configurar(int quantidadeDisponivel, int quantidadeMinima)
    {
        if (quantidadeDisponivel < 0)
            throw new ArgumentException("Quantidade disponivel nao pode ser negativa.", nameof(quantidadeDisponivel));
        if (quantidadeMinima < 0)
            throw new ArgumentException("Quantidade minima nao pode ser negativa.", nameof(quantidadeMinima));

        QuantidadeDisponivel = quantidadeDisponivel;
        QuantidadeMinima = quantidadeMinima;
    }

    public void RegistrarEntrada(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade de entrada deve ser maior que zero.", nameof(quantidade));

        QuantidadeDisponivel += quantidade;
    }

    public void RegistrarSaida(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade de saida deve ser maior que zero.", nameof(quantidade));
        if (QuantidadeDisponivel < quantidade)
            throw new InvalidOperationException("Estoque insuficiente para a saida informada.");

        QuantidadeDisponivel -= quantidade;
    }

    public void Ajustar(int novaQuantidade)
    {
        if (novaQuantidade < 0)
            throw new ArgumentException("Nova quantidade nao pode ser negativa.", nameof(novaQuantidade));

        QuantidadeDisponivel = novaQuantidade;
    }
}
