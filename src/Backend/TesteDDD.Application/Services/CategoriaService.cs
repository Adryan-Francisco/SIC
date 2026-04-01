using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;

namespace TesteDDD.Application.Services;

public interface ICategoriaService
{
    Task<ResponseCategoriaJson> CreateAsync(RequestCategoriaJson request);
    Task<IList<ResponseCategoriaJson>> GetAllAsync();
    Task<ResponseCategoriaJson?> GetByIdAsync(Guid id);
    Task<ResponseCategoriaJson?> UpdateAsync(Guid id, RequestCategoriaJson request);
    Task<bool> DeleteAsync(Guid id);
}

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _repository;

    public CategoriaService(ICategoriaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseCategoriaJson> CreateAsync(RequestCategoriaJson request)
    {
        var categoria = new Categoria(request.Name, request.Descricao);

        await _repository.AddAsync(categoria);

        return new ResponseCategoriaJson
        {
            Id = categoria.Id,
            Name = categoria.Name,
            Descricao = categoria.Descricao
        };
    }

    public async Task<IList<ResponseCategoriaJson>> GetAllAsync()
    {
        var categorias = await _repository.GetAllAsync();

        return categorias.Select(c => new ResponseCategoriaJson
        {
            Id = c.Id,
            Name = c.Name,
            Descricao = c.Descricao
        }).ToList();
    }

    public async Task<ResponseCategoriaJson?> GetByIdAsync(Guid id)
    {
        var categoria = await _repository.GetByIdAsync(id);

        if (categoria == null)
            return null;

        return new ResponseCategoriaJson
        {
            Id = categoria.Id,
            Name = categoria.Name,
            Descricao = categoria.Descricao
        };
    }

    public async Task<ResponseCategoriaJson?> UpdateAsync(Guid id, RequestCategoriaJson request)
    {
        var categoria = await _repository.GetByIdAsync(id);

        if (categoria == null)
            return null;

        categoria.Update(request.Name, request.Descricao);

        await _repository.UpdateAsync(categoria);

        return new ResponseCategoriaJson
        {
            Id = categoria.Id,
            Name = categoria.Name,
            Descricao = categoria.Descricao
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var categoria = await _repository.GetByIdAsync(id);

        if (categoria == null)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }
}