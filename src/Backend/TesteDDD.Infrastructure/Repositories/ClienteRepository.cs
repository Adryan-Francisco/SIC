using Microsoft.EntityFrameworkCore;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;

namespace TesteDDD.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _dbContext;

        public ClienteRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Cliente cliente)
        {
            // Ajustado para .Clientes (plural)
            await _dbContext.Clientes.AddAsync(cliente);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            // O AsNoTracking() é ótimo para performance em consultas de leitura!
            return await _dbContext.Clientes.AsNoTracking().ToListAsync();
        }

        public async Task<Cliente?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Clientes.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdateAsync(Cliente cliente) // Ajustado o tipo para 'Cliente'
        {
            _dbContext.Clientes.Update(cliente);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var cliente = await GetByIdAsync(id);
            if (cliente is null) return;

            _dbContext.Clientes.Remove(cliente);
            await _dbContext.SaveChangesAsync();
        }
    }
}