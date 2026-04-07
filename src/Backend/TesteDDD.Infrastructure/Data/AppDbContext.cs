using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TesteDDD.Domain.Entities;


namespace TesteDDD.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos { get; set; }

    public DbSet<Categoria> Categorias { get; set; }

    public DbSet<Cliente> Clientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>()
       .Property(p => p.Preco)
       .HasPrecision(18, 2);

        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId);

        base.OnModelCreating(modelBuilder);


       
    }
}