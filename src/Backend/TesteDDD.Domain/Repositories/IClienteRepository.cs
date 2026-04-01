using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteDDD.Domain.Entities;

namespace TesteDDD.Domain.Repositories
{
    public interface IClienteRepository
    {
        Task AddAsync(Cliente cliente);
        Task<IEnumerable<Cliente>> GetAllAsync();
        Task<Cliente?> GetByIdAsync(Guid id);
        Task UpdateAsync(Cliente cliente);
        Task DeleteAsync(Guid id);
    }
}
