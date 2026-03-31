using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;
using TesteDDD.Domain.Repositories;

namespace TesteDDD.Application.Services;

public interface ICategoriaService
{
    Task<ResponseCategoriaJson> CreateAsync(RequestCategoriaJson request);
    Task<IEnumerable<ResponseCategoriaJson>> GetAllAsync();
    Task<ResponseCategoriaJson> GetByIdAsync(Guid id);
    Task<ResponseCategoriaJson> UpdateAsync(Guid id, RequestCategoriaJson request);
    Task DeleteAsync(Guid id);
}

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _repository;

    public CategoriaService(ICategoriaRepository repository)
    {
        _repository = repository;
    }


    // ✅ CREATE
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

    // ✅ GET ALL
    public async Task<IEnumerable<ResponseCategoriaJson>> GetAllAsync()
    {
        var categorias = await _repository.GetAllAsync();

        return categorias.Select(c => new ResponseCategoriaJson
        {
            Id = c.Id,
            Name = c.Name,
            Descricao = c.Descricao
        });
    }

    // ✅ GET BY ID
    public async Task<ResponseCategoriaJson> GetByIdAsync(Guid id)
    {
        var categoria = await _repository.GetByIdAsync(id);

        if (categoria == null)
            throw new KeyNotFoundException($"Categoria com ID {id} não encontrada.");

        return new ResponseCategoriaJson
        {
            Id = categoria.Id,
            Name = categoria.Name,
            Descricao = categoria.Descricao
        };
    }

    public async Task<ResponseCategoriaJson> UpdateAsync(Guid id, RequestCategoriaJson request)
    {
        var categoria = await _repository.GetByIdAsync(id);

        if (categoria == null)
            throw new KeyNotFoundException($"Categoria com ID {id} não encontrada.");

        categoria.Update(request.Name, request.Descricao);

        await _repository.UpdateAsync(categoria);

        return new ResponseCategoriaJson
        {
            Id = categoria.Id,
            Name = categoria.Name,
            Descricao = categoria.Descricao
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        var categoria = await _repository.GetByIdAsync(id);

        if (categoria == null)
            throw new KeyNotFoundException($"Categoria com ID {id} não encontrada.");

        await _repository.DeleteAsync(id);
    }
}