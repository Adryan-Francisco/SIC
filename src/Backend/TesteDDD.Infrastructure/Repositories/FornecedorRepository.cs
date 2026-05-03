using Microsoft.EntityFrameworkCore;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;

namespace TesteDDD.Infrastructure.Repositories;

public class FornecedorRepository : IFornecedorRepository
{
    private readonly AppDbContext _dbContext;

    public FornecedorRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Fornecedor fornecedor)
    {
        await _dbContext.Fornecedores.AddAsync(fornecedor);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Fornecedor>> GetAllAsync()
    {
        return await _dbContext.Fornecedores.AsNoTracking().ToListAsync();
    }

    public async Task<Fornecedor?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Fornecedores.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(Fornecedor fornecedor)
    {
        _dbContext.Fornecedores.Update(fornecedor);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var fornecedor = await GetByIdAsync(id);
        if (fornecedor == null)
            return;

        _dbContext.Fornecedores.Remove(fornecedor);
        await _dbContext.SaveChangesAsync();
    }
}
