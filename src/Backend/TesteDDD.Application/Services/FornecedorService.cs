using AutoMapper;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;

namespace TesteDDD.Application.Services;

public interface IFornecedorService
{
    Task<ResponseFornecedorJson> CreateAsync(RequestFornecedorJson request);
    Task<IList<ResponseFornecedorJson>> GetAllAsync();
    Task<ResponseFornecedorJson?> GetByIdAsync(Guid id);
    Task<ResponseFornecedorJson?> UpdateAsync(Guid id, RequestFornecedorJson request);
    Task<bool> DeleteAsync(Guid id);
}

public class FornecedorService : IFornecedorService
{
    private readonly IFornecedorRepository _repository;
    private readonly IMapper _mapper;

    public FornecedorService(IFornecedorRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ResponseFornecedorJson> CreateAsync(RequestFornecedorJson request)
    {
        var fornecedor = new Fornecedor(request.Nome, request.Documento, request.Email, request.Telefone);
        await _repository.AddAsync(fornecedor);
        return _mapper.Map<ResponseFornecedorJson>(fornecedor);
    }

    public async Task<IList<ResponseFornecedorJson>> GetAllAsync()
    {
        var fornecedores = await _repository.GetAllAsync();
        return _mapper.Map<IList<ResponseFornecedorJson>>(fornecedores);
    }

    public async Task<ResponseFornecedorJson?> GetByIdAsync(Guid id)
    {
        var fornecedor = await _repository.GetByIdAsync(id);
        return fornecedor == null ? null : _mapper.Map<ResponseFornecedorJson>(fornecedor);
    }

    public async Task<ResponseFornecedorJson?> UpdateAsync(Guid id, RequestFornecedorJson request)
    {
        var fornecedor = await _repository.GetByIdAsync(id);
        if (fornecedor == null)
            return null;

        fornecedor.Update(request.Nome, request.Documento, request.Email, request.Telefone);
        await _repository.UpdateAsync(fornecedor);
        return _mapper.Map<ResponseFornecedorJson>(fornecedor);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var fornecedor = await _repository.GetByIdAsync(id);
        if (fornecedor == null)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }
}
