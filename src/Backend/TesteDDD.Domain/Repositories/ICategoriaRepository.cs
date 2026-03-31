using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteDDD.Domain.Entities;

namespace TesteDDD.Domain.Repositories
{
     public interface ICategoriaRepository
    {
        Task AddAsync(Categoria categoria);
        Task<IEnumerable<Categoria>> GetAllAsync();
        Task<Categoria?> GetByIdAsync(Guid id);
        Task UpdateAsync(Categoria categoria);
        Task DeleteAsync(Guid id);
    }
}
