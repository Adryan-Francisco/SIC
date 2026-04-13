namespace TesteDDD.Domain.Entities;

public class Produto
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public Guid CategoriaId { get; private set; }
    public Categoria? Categoria { get; private set; }

    protected Produto()
    {
    }

    public Produto(string nome, decimal preco, Guid categoriaId)
    {
        Id = Guid.NewGuid();
        Update(nome, preco, categoriaId);
    }

    public void Update(string nome, decimal preco, Guid categoriaId)
    {
        Nome = nome;
        Preco = preco;
        CategoriaId = categoriaId;
    }
}
