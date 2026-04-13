using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteDDD.Domain.Entities;

namespace TesteDDD.Domain.Repositories
{
    public interface IVendasRepository
    {
        Task AddAsync(Vendas venda);
        Task<IEnumerable<Vendas>> GetAllAsync();
        Task<Vendas?> GetByIdAsync(Guid id);
        Task UpdateAsync(Vendas venda);
        Task DeleteAsync(Guid id);
    }
}
