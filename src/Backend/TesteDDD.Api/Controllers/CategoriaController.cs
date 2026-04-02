using Microsoft.AspNetCore.Mvc;
using TesteDDD.Application.Services;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriaController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriaController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RequestCategoriaJson request)
    {
        var response = await _categoriaService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _categoriaService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await _categoriaService.GetByIdAsync(id);

        if (response is null)
            return NotFound(new { Message = $"Categoria com ID {id} não encontrada." });

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] RequestCategoriaJson request)
    {
        var response = await _categoriaService.UpdateAsync(id, request);

        if (response is null)
            return NotFound(new { Message = $"Categoria com ID {id} não encontrada." });

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await _categoriaService.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { Message = $"Categoria com ID {id} não encontrada." });

        return NoContent();
    }
}