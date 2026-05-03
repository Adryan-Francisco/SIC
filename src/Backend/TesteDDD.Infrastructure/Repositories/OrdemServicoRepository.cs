using Microsoft.EntityFrameworkCore;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;

namespace TesteDDD.Infrastructure.Repositories;

public class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly AppDbContext _dbContext;

    public OrdemServicoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OrdemServico ordemServico)
    {
        await _dbContext.OrdensServico.AddAsync(ordemServico);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<OrdemServico>> GetAllAsync()
    {
        return await _dbContext.OrdensServico
            .Include(x => x.Cliente)
            .Include(x => x.Produto)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<OrdemServico?> GetByIdAsync(Guid id)
    {
        return await _dbContext.OrdensServico
            .Include(x => x.Cliente)
            .Include(x => x.Produto)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(OrdemServico ordemServico)
    {
        _dbContext.OrdensServico.Update(ordemServico);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var ordemServico = await GetByIdAsync(id);
        if (ordemServico == null)
            return;

        _dbContext.OrdensServico.Remove(ordemServico);
        await _dbContext.SaveChangesAsync();
    }
}
