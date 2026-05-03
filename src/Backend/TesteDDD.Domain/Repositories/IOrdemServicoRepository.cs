using TesteDDD.Domain.Entities;

namespace TesteDDD.Domain.Repositories;

public interface IOrdemServicoRepository
{
    Task AddAsync(OrdemServico ordemServico);
    Task<IEnumerable<OrdemServico>> GetAllAsync();
    Task<OrdemServico?> GetByIdAsync(Guid id);
    Task UpdateAsync(OrdemServico ordemServico);
    Task DeleteAsync(Guid id);
}
