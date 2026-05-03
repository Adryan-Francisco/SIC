using TesteDDD.Domain.Entities;

namespace TesteDDD.Domain.Repositories;

public interface IEstoqueRepository
{
    Task AddAsync(Estoque estoque);
    Task<IEnumerable<Estoque>> GetAllAsync();
    Task<Estoque?> GetByIdAsync(Guid id);
    Task<Estoque?> GetByProdutoIdAsync(Guid produtoId);
    Task UpdateAsync(Estoque estoque);
}
