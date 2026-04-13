namespace TesteDDD.Domain.Entities;

public class Categoria
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public ICollection<Produto> Produtos { get; private set; } = new List<Produto>();

    protected Categoria()
    {
    }

    public Categoria(string name, string descricao)
    {
        Id = Guid.NewGuid();
        Update(name, descricao);
    }

    public void Update(string name, string descricao)
    {
        Name = name;
        Descricao = descricao;
    }
}
