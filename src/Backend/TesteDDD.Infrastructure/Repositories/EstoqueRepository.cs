using Microsoft.EntityFrameworkCore;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;

namespace TesteDDD.Infrastructure.Repositories;

public class EstoqueRepository : IEstoqueRepository
{
    private readonly AppDbContext _dbContext;

    public EstoqueRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Estoque estoque)
    {
        await _dbContext.Estoques.AddAsync(estoque);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Estoque>> GetAllAsync()
    {
        return await _dbContext.Estoques
            .Include(x => x.Produto)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Estoque?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Estoques
            .Include(x => x.Produto)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Estoque?> GetByProdutoIdAsync(Guid produtoId)
    {
        return await _dbContext.Estoques
            .Include(x => x.Produto)
            .FirstOrDefaultAsync(x => x.ProdutoId == produtoId);
    }

    public async Task UpdateAsync(Estoque estoque)
    {
        _dbContext.Estoques.Update(estoque);
        await _dbContext.SaveChangesAsync();
    }
}
