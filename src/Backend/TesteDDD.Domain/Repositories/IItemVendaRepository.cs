using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteDDD.Domain.Entities;

namespace TesteDDD.Domain.Repositories
{
    public interface IItemVendaRepository
    {
        Task AddAsync(ItemVendas itemVenda);
        Task<IEnumerable<ItemVendas>> GetAllAsync();
        Task<ItemVendas?> GetByIdAsync(Guid id);
        Task UpdateAsync(ItemVendas itemVenda);
        Task DeleteAsync(Guid id);

    }
}
