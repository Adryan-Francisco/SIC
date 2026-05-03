using TesteDDD.Domain.Entities;

namespace TesteDDD.Domain.Repositories;

public interface IFornecedorRepository
{
    Task AddAsync(Fornecedor fornecedor);
    Task<IEnumerable<Fornecedor>> GetAllAsync();
    Task<Fornecedor?> GetByIdAsync(Guid id);
    Task UpdateAsync(Fornecedor fornecedor);
    Task DeleteAsync(Guid id);
}
