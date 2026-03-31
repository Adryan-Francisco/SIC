using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TesteDDD.Domain.Entities; // Ajuste para o namespace correto da sua entidade

namespace TesteDDD.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos { get; set; }

    public DbSet<Categoria> Categorias { get; set; }
}