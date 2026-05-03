namespace TesteDDD.Domain.Entities;

public class Fornecedor
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Documento { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;
    public List<Produto> Produtos { get; private set; } = new();

    protected Fornecedor()
    {
    }

    public Fornecedor(string nome, string documento, string email, string telefone)
    {
        Id = Guid.NewGuid();
        Update(nome, documento, email, telefone);
    }

    public void Update(string nome, string documento, string email, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do fornecedor e obrigatorio.", nameof(nome));
        if (string.IsNullOrWhiteSpace(documento))
            throw new ArgumentException("Documento do fornecedor e obrigatorio.", nameof(documento));

        Nome = nome;
        Documento = documento;
        Email = email?.Trim() ?? string.Empty;
        Telefone = telefone?.Trim() ?? string.Empty;
    }
}
