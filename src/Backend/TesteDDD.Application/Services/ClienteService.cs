using AutoMapper;
using TesteDDD.Application.Exceptions;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;

namespace TesteDDD.Application.Services
{
    public interface IClienteService
    {
        Task<ResponseClienteJson> CreateAsync(RequestClienteJson request);
        Task<IList<ResponseClienteJson>> GetAllAsync();
        Task<ResponseClienteJson?> GetByIdAsync(Guid id);
        Task<ResponseClienteJson?> UpdateAsync(Guid id, RequestClienteJson request);
        Task<bool> DeleteAsync(Guid id);
    }

    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _repository;
        private readonly IMapper _mapper;

        public ClienteService(IClienteRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseClienteJson> CreateAsync(RequestClienteJson request)
        {
            var cliente = new Cliente(Guid.NewGuid(), request.Nome, request.Endereco, request.Cep);
            await _repository.AddAsync(cliente);

            return _mapper.Map<ResponseClienteJson>(cliente);
        }

        public async Task<IList<ResponseClienteJson>> GetAllAsync()
        {
            var clientes = await _repository.GetAllAsync();
            return _mapper.Map<IList<ResponseClienteJson>>(clientes);
        }

        public async Task<ResponseClienteJson?> GetByIdAsync(Guid id)
        {
            var cliente = await _repository.GetByIdAsync(id);
            return cliente == null ? null : _mapper.Map<ResponseClienteJson>(cliente);
        }

        public async Task<ResponseClienteJson?> UpdateAsync(Guid id, RequestClienteJson request)
        {
            var cliente = await _repository.GetByIdAsync(id);
            if (cliente == null)
                return null;

            cliente.Update(request.Nome, request.Endereco, request.Cep);
            await _repository.UpdateAsync(cliente);

            return _mapper.Map<ResponseClienteJson>(cliente);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var cliente = await _repository.GetByIdAsync(id);
            if (cliente == null)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
