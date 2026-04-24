using TesteDDD.Domain.Entities;

namespace TesteDDD.Domain.Repositories;

public interface INotaFiscalRepository
{
    Task AdicionarAsync(NotaFiscal notaFiscal);
    Task AtualizarAsync(NotaFiscal notaFiscal);
    Task<NotaFiscal?> ObterPorIdAsync(Guid id);
    Task<NotaFiscal?> ObterPorVendasIdAsync(Guid vendasId);
    Task<NotaFiscal?> ObterPorChaveAcessoAsync(string chaveAcesso);
    Task<List<NotaFiscal>> ListarPorStatusAsync(int status);
    Task DeletarAsync(Guid id);
}
