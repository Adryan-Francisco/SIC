using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;

namespace TesteDDD.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        // AppDbContext é a sua classe que herda de DbContext do EF Core
        private readonly AppDbContext _dbContext;

        public CategoriaRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Categoria categoria)
        {
            await _dbContext.Categorias.AddAsync(categoria);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Categoria>> GetAllAsync()
        {
            return await _dbContext.Categorias.AsNoTracking().ToListAsync();
        }

        public async Task<Produto?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Categorias.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdateAsync(Categoria categoria)
        {
            _dbContext.Categorias.Update(categoria);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var categoria = await GetByIdAsync(id);
            if (categoria is null) return;

            _dbContext..Remove(categoria);
            await _dbContext.SaveChangesAsync();
        }
    }
}
