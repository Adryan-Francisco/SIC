using Microsoft.EntityFrameworkCore;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;

namespace TesteDDD.Infrastructure.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly AppDbContext _dbContext;

    public ProdutoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Produto produto)
    {
        await _dbContext.Produtos.AddAsync(produto);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Produto>> GetAllAsync()
    {
        return await _dbContext.Produtos
            .Include(p => p.Fornecedor)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Produto?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Produtos
            .Include(p => p.Fornecedor)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task UpdateAsync(Produto produto)
    {
        _dbContext.Produtos.Update(produto);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var produto = await GetByIdAsync(id);
        if (produto == null)
            return;

        _dbContext.Produtos.Remove(produto);
        await _dbContext.SaveChangesAsync();
    }
}
