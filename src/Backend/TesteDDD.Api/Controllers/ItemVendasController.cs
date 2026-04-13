using Microsoft.AspNetCore.Mvc;
using TesteDDD.Application.Services;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemVendasController : ControllerBase
{
    private readonly IItemVendasService _itemVendasService;

    public ItemVendasController(IItemVendasService itemVendasService)
    {
        _itemVendasService = itemVendasService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RequestItemVendasJson request)
    {
        var response = await _itemVendasService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _itemVendasService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await _itemVendasService.GetByIdAsync(id);
        if (response is null)
            return NotFound(new { Message = $"ItemVenda com ID {id} nao encontrado." });

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] RequestItemVendasJson request)
    {
        var response = await _itemVendasService.UpdateAsync(id, request);
        if (response is null)
            return NotFound(new { Message = $"ItemVenda com ID {id} nao encontrado." });

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await _itemVendasService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { Message = $"ItemVenda com ID {id} nao encontrado." });

        return NoContent();
    }
}
