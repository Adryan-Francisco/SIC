namespace TesteDDD.Domain.Entities;

public class Produto
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public Guid CategoriaId { get; private set; }
    public Categoria Categoria { get; private set; } = null!;
    public Guid FornecedorId { get; private set; }
    public Fornecedor Fornecedor { get; private set; } = null!;

    protected Produto()
    {
    }

    public Produto(string nome, decimal preco, Guid categoriaId, Guid fornecedorId)
    {
        Id = Guid.NewGuid();
        Update(nome, preco, categoriaId, fornecedorId);
    }

    public void Update(string nome, decimal preco, Guid categoriaId, Guid fornecedorId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome e obrigatorio.", nameof(nome));
        if (preco <= 0)
            throw new ArgumentException("Preco deve ser maior que zero.", nameof(preco));
        if (categoriaId == Guid.Empty)
            throw new ArgumentException("CategoriaId e obrigatorio.", nameof(categoriaId));
        if (fornecedorId == Guid.Empty)
            throw new ArgumentException("FornecedorId e obrigatorio.", nameof(fornecedorId));

        Nome = nome;
        Preco = preco;
        CategoriaId = categoriaId;
        FornecedorId = fornecedorId;
    }

    public void DefinirCategoria(Categoria categoria)
    {
        if (categoria == null)
            throw new ArgumentNullException(nameof(categoria));

        Categoria = categoria;
        CategoriaId = categoria.Id;
    }

    public void DefinirFornecedor(Fornecedor fornecedor)
    {
        if (fornecedor == null)
            throw new ArgumentNullException(nameof(fornecedor));

        Fornecedor = fornecedor;
        FornecedorId = fornecedor.Id;
    }
}
