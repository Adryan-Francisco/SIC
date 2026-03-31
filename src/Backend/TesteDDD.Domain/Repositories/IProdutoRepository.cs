using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteDDD.Domain.Entities;

namespace TesteDDD.Domain.Repositories
{
    public interface IProdutoRepository
    {
        Task AddAsync(Produto produto);
        Task<IEnumerable<Produto>> GetAllAsync();
        Task<Produto?> GetByIdAsync(Guid id);
        Task UpdateAsync(Produto produto);
        Task DeleteAsync(Guid id);
    }
}

