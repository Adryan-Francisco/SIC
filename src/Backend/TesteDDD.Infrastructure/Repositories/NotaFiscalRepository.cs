using Microsoft.EntityFrameworkCore;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;

namespace TesteDDD.Infrastructure.Repositories;

public class NotaFiscalRepository : INotaFiscalRepository
{
    private readonly AppDbContext _context;

    public NotaFiscalRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AdicionarAsync(NotaFiscal notaFiscal)
    {
        if (notaFiscal == null)
            throw new ArgumentNullException(nameof(notaFiscal));

        await _context.NotasFiscais.AddAsync(notaFiscal);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(NotaFiscal notaFiscal)
    {
        if (notaFiscal == null)
            throw new ArgumentNullException(nameof(notaFiscal));

        _context.NotasFiscais.Update(notaFiscal);
        await _context.SaveChangesAsync();
    }

    public async Task<NotaFiscal?> ObterPorIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id não pode ser vazio.", nameof(id));

        return await _context.NotasFiscais
            .Include(nf => nf.Vendas)
            .FirstOrDefaultAsync(nf => nf.Id == id);
    }

    public async Task<NotaFiscal?> ObterPorVendasIdAsync(Guid vendasId)
    {
        if (vendasId == Guid.Empty)
            throw new ArgumentException("VendasId não pode ser vazio.", nameof(vendasId));

        return await _context.NotasFiscais
            .Include(nf => nf.Vendas)
            .FirstOrDefaultAsync(nf => nf.VendasId == vendasId);
    }

    public async Task<NotaFiscal?> ObterPorChaveAcessoAsync(string chaveAcesso)
    {
        if (string.IsNullOrWhiteSpace(chaveAcesso))
            throw new ArgumentException("Chave de acesso não pode ser vazia.", nameof(chaveAcesso));

        return await _context.NotasFiscais
            .Include(nf => nf.Vendas)
            .FirstOrDefaultAsync(nf => nf.ChaveAcesso == chaveAcesso);
    }

    public async Task<List<NotaFiscal>> ListarPorStatusAsync(int status)
    {
        return await _context.NotasFiscais
            .Include(nf => nf.Vendas)
            .Where(nf => (int)nf.Status == status)
            .ToListAsync();
    }

    public async Task DeletarAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id não pode ser vazio.", nameof(id));

        var notaFiscal = await _context.NotasFiscais.FindAsync(id);
        if (notaFiscal != null)
        {
            _context.NotasFiscais.Remove(notaFiscal);
            await _context.SaveChangesAsync();
        }
    }
}
