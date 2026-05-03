using TesteDDD.Domain.Entities;

namespace TesteDDD.Domain.Repositories;

public interface IMovimentacaoEstoqueRepository
{
    Task AddAsync(MovimentacaoEstoque movimentacao);
    Task<IEnumerable<MovimentacaoEstoque>> GetAllAsync();
    Task<IEnumerable<MovimentacaoEstoque>> GetByProdutoIdAsync(Guid produtoId);
}
