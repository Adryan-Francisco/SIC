using Microsoft.EntityFrameworkCore;
using TesteDDD.Domain.Entities;

namespace TesteDDD.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Vendas> Vendas { get; set; }
    public DbSet<ItemVendas> ItensVendas { get; set; }
    public DbSet<NotaFiscal> NotasFiscais { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<OrdemServico> OrdensServico { get; set; }
    public DbSet<Estoque> Estoques { get; set; }
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>()
            .Property(p => p.Preco)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Fornecedor)
            .WithMany(f => f.Produtos)
            .HasForeignKey(p => p.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Fornecedor>()
            .Property(f => f.Nome)
            .HasMaxLength(150);

        modelBuilder.Entity<Fornecedor>()
            .Property(f => f.Documento)
            .HasMaxLength(20);

        modelBuilder.Entity<Fornecedor>()
            .Property(f => f.Email)
            .HasMaxLength(150);

        modelBuilder.Entity<Fornecedor>()
            .Property(f => f.Telefone)
            .HasMaxLength(20);

        modelBuilder.Entity<ItemVendas>()
            .Property(iv => iv.PrecoUnitario)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Vendas>()
            .Property(v => v.ValorTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Vendas>()
            .HasMany(v => v.Itens)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NotaFiscal>()
            .HasOne(nf => nf.Vendas)
            .WithMany()
            .HasForeignKey(nf => nf.VendasId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NotaFiscal>()
            .Property(nf => nf.Status)
            .HasConversion<int>();

        modelBuilder.Entity<OrdemServico>()
            .Property(os => os.ValorServico)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrdemServico>()
            .Property(os => os.Status)
            .HasConversion<int>();

        modelBuilder.Entity<OrdemServico>()
            .HasOne(os => os.Cliente)
            .WithMany()
            .HasForeignKey(os => os.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdemServico>()
            .HasOne(os => os.Produto)
            .WithMany()
            .HasForeignKey(os => os.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Estoque>()
            .HasIndex(e => e.ProdutoId)
            .IsUnique();

        modelBuilder.Entity<Estoque>()
            .HasOne(e => e.Produto)
            .WithMany()
            .HasForeignKey(e => e.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimentacaoEstoque>()
            .Property(m => m.Tipo)
            .HasConversion<int>();

        modelBuilder.Entity<MovimentacaoEstoque>()
            .Property(m => m.Observacao)
            .HasMaxLength(250);

        modelBuilder.Entity<MovimentacaoEstoque>()
            .HasOne(m => m.Produto)
            .WithMany()
            .HasForeignKey(m => m.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
