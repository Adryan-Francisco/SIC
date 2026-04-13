namespace TesteDDD.Domain.Entities;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Endereco { get; private set; } = string.Empty;
    public string Cep { get; private set; } = string.Empty;

    protected Cliente()
    {
    }

    public Cliente(Guid id, string nome, string endereco, string cep)
    {
        Id = id;
        Update(nome, endereco, cep);
    }

    public void Update(string nome, string endereco, string cep)
    {
        Nome = nome;
        Endereco = endereco;
        Cep = cep;
    }
}
