using Microsoft.EntityFrameworkCore;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;

namespace TesteDDD.Infrastructure.Repositories;

public class ItemVendasRepository : IItemVendaRepository
{
    private readonly AppDbContext _dbContext;

    public ItemVendasRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ItemVendas itemVenda)
    {
        await _dbContext.ItensVendas.AddAsync(itemVenda);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<ItemVendas>> GetAllAsync()
    {
        return await _dbContext.ItensVendas
            .Include(i => i.Produto)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ItemVendas?> GetByIdAsync(Guid id)
    {
        return await _dbContext.ItensVendas
            .Include(i => i.Produto)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task UpdateAsync(ItemVendas itemVenda)
    {
        _dbContext.ItensVendas.Update(itemVenda);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var itemVenda = await GetByIdAsync(id);
        if (itemVenda is null)
            return;

        _dbContext.ItensVendas.Remove(itemVenda);
        await _dbContext.SaveChangesAsync();
    }
}
