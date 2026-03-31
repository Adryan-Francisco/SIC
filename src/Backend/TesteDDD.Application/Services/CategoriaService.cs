using System;
using System.Collections.Generic;
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
    // Adicione os métodos GetById, Update e Delete na interface...
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
        // 1. Mapear Request para Entidade
        var categoria = new Categoria(request.Name, request.Descricao);

        // 2. Persistir
        await _repository.AddAsync(categoria);

        // 3. Retornar Response
        return new ResponseCategoriaJson
        {
            Id = categoria.Id,
            Name = categoria.Name,
            Descricao = categoria.Descricao
        };
    }

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

    // Implemente os demais métodos (Update, Delete, GetById) seguindo essa lógica...
}
