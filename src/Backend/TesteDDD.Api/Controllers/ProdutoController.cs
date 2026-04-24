using Microsoft.AspNetCore.Mvc;
using TesteDDD.Application.Services;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProdutoController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutoController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RequestProdutoJson request)
    {
        var response = await _produtoService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _produtoService.GetAllAsync();
        return Ok(response);
    }

  
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await _produtoService.GetByIdAsync(id);

        if (response is null)
            return NotFound(new { Message = $"Produto com ID {id} não encontrado." });

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] RequestProdutoJson request)
    {
        var response = await _produtoService.UpdateAsync(id, request);

        if (response is null)
            return NotFound(new { Message = $"Produto com ID {id} não encontrado." });

        return Ok(response);
    }

   
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await _produtoService.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { Message = $"Produto com ID {id} não encontrado." });

        return NoContent();
    }
}