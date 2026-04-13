using Microsoft.AspNetCore.Mvc;
using TesteDDD.Application.Services;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VendasController : ControllerBase
{
    private readonly IVendasService _vendasService;

    public VendasController(IVendasService vendasService)
    {
        _vendasService = vendasService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RequestVendasJson request)
    {
        var response = await _vendasService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _vendasService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await _vendasService.GetByIdAsync(id);
        if (response is null)
            return NotFound(new { Message = $"Venda com ID {id} nao encontrada." });

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] RequestVendasJson request)
    {
        var response = await _vendasService.UpdateAsync(id, request);
        if (response is null)
            return NotFound(new { Message = $"Venda com ID {id} nao encontrada." });

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await _vendasService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { Message = $"Venda com ID {id} nao encontrada." });

        return NoContent();
    }
}
