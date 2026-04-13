using Microsoft.EntityFrameworkCore;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;

namespace TesteDDD.Infrastructure.Repositories;

public class VendasRepository : IVendasRepository
{
    private readonly AppDbContext _dbContext;

    public VendasRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Vendas venda)
    {
        await _dbContext.Vendas.AddAsync(venda);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Vendas>> GetAllAsync()
    {
        return await _dbContext.Vendas
            .Include(v => v.Itens)
            .ThenInclude(i => i.Produto)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Vendas?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Vendas
            .Include(v => v.Itens)
            .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task UpdateAsync(Vendas venda)
    {
        _dbContext.Vendas.Update(venda);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var venda = await GetByIdAsync(id);
        if (venda is null)
            return;

        _dbContext.Vendas.Remove(venda);
        await _dbContext.SaveChangesAsync();
    }
}
