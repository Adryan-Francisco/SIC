using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;
using TesteDDD.Application.Exceptions;

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

        public ClienteService(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<ResponseClienteJson> CreateAsync(RequestClienteJson request)
        {
            ValidateRequest(request);

            var cliente = new Cliente(Guid.NewGuid(), request.Nome, request.Endereco, request.Cep);

            await _repository.AddAsync(cliente);

            return new ResponseClienteJson
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Endereco = cliente.Endereco,
                Cep = cliente.Cep
            };
        }

        public async Task<IList<ResponseClienteJson>> GetAllAsync()
        {
            var clientes = await _repository.GetAllAsync();

            return clientes.Select(c => new ResponseClienteJson
            {
                Id = c.Id,
                Nome = c.Nome,
                Endereco = c.Endereco,
                Cep = c.Cep
            }).ToList();
        }

        public async Task<ResponseClienteJson?> GetByIdAsync(Guid id)
        {
            var cliente = await _repository.GetByIdAsync(id);

            if (cliente == null)
                return null;

            return new ResponseClienteJson
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Endereco = cliente.Endereco,
                Cep = cliente.Cep 
            };
        }

        public async Task<ResponseClienteJson?> UpdateAsync(Guid id, RequestClienteJson request)
        {
            ValidateRequest(request);

            var cliente = await _repository.GetByIdAsync(id);

            if (cliente == null)
                return null;

            cliente.Update(request.Nome, request.Endereco, request.Cep);

            await _repository.UpdateAsync(cliente);

            return new ResponseClienteJson
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Endereco = cliente.Endereco,
                Cep = cliente.Cep
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var cliente = await _repository.GetByIdAsync(id);

            if (cliente == null)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        private static void ValidateRequest(RequestClienteJson request)
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
                throw new BusinessRuleException("CLIENTE_NOME_INVALIDO", "Nome do cliente e obrigatorio.");

            if (request.Nome.Trim().Length < 3)
                throw new BusinessRuleException("CLIENTE_NOME_CURTO", "Nome do cliente deve ter pelo menos 3 caracteres.");

            if (string.IsNullOrWhiteSpace(request.Endereco))
                throw new BusinessRuleException("CLIENTE_ENDERECO_INVALIDO", "Endereco do cliente e obrigatorio.");

            if (string.IsNullOrWhiteSpace(request.Cep))
                throw new BusinessRuleException("CLIENTE_CEP_INVALIDO", "CEP do cliente e obrigatorio.");
        }
    }

    }
