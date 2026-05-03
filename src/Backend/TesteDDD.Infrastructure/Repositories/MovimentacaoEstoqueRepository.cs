using Microsoft.EntityFrameworkCore;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;

namespace TesteDDD.Infrastructure.Repositories;

public class MovimentacaoEstoqueRepository : IMovimentacaoEstoqueRepository
{
    private readonly AppDbContext _dbContext;

    public MovimentacaoEstoqueRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(MovimentacaoEstoque movimentacao)
    {
        await _dbContext.MovimentacoesEstoque.AddAsync(movimentacao);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<MovimentacaoEstoque>> GetAllAsync()
    {
        return await _dbContext.MovimentacoesEstoque
            .Include(x => x.Produto)
            .AsNoTracking()
            .OrderByDescending(x => x.DataMovimentacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<MovimentacaoEstoque>> GetByProdutoIdAsync(Guid produtoId)
    {
        return await _dbContext.MovimentacoesEstoque
            .Include(x => x.Produto)
            .Where(x => x.ProdutoId == produtoId)
            .AsNoTracking()
            .OrderByDescending(x => x.DataMovimentacao)
            .ToListAsync();
    }
}
