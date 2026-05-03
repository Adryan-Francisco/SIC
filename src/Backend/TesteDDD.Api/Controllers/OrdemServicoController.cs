using Microsoft.AspNetCore.Mvc;
using TesteDDD.Application.Services;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdemServicoController : ControllerBase
{
    private readonly IOrdemServicoService _ordemServicoService;

    public OrdemServicoController(IOrdemServicoService ordemServicoService)
    {
        _ordemServicoService = ordemServicoService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RequestOrdemServicoJson request)
    {
        var response = await _ordemServicoService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _ordemServicoService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await _ordemServicoService.GetByIdAsync(id);
        return response == null ? NotFound() : Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] RequestOrdemServicoJson request)
    {
        var response = await _ordemServicoService.UpdateAsync(id, request);
        return response == null ? NotFound() : Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        return await _ordemServicoService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
