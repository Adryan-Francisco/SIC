using Microsoft.AspNetCore.Mvc;
using TesteDDD.Application.Services;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClienteController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RequestClienteJson request)
    {
        var response = await _clienteService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _clienteService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await _clienteService.GetByIdAsync(id);
            
        if (response is null)
            return NotFound(new { Message = $"Cliente com ID {id} não encontrada." });

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] RequestClienteJson request)
    {
        var response = await _clienteService.UpdateAsync(id, request);

        if (response is null)
            return NotFound(new { Message = $"Cliente com ID {id} não encontrada." });

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await _clienteService.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { Message = $"Cliente com ID {id} não encontrada." });

        return NoContent();
    }
}