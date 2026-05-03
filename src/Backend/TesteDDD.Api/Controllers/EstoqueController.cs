using Microsoft.AspNetCore.Mvc;
using TesteDDD.Application.Services;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EstoqueController : ControllerBase
{
    private readonly IEstoqueService _estoqueService;

    public EstoqueController(IEstoqueService estoqueService)
    {
        _estoqueService = estoqueService;
    }

    [HttpPut]
    public async Task<IActionResult> Configurar([FromBody] RequestEstoqueJson request)
    {
        return Ok(await _estoqueService.ConfigurarAsync(request));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _estoqueService.GetAllAsync());
    }

    [HttpGet("produto/{produtoId:guid}")]
    public async Task<IActionResult> GetByProdutoId([FromRoute] Guid produtoId)
    {
        var response = await _estoqueService.GetByProdutoIdAsync(produtoId);
        return response == null ? NotFound() : Ok(response);
    }

    [HttpPost("entrada")]
    public async Task<IActionResult> RegistrarEntrada([FromBody] RequestMovimentacaoEstoqueJson request)
    {
        return Ok(await _estoqueService.RegistrarEntradaAsync(request));
    }

    [HttpPost("saida")]
    public async Task<IActionResult> RegistrarSaida([FromBody] RequestMovimentacaoEstoqueJson request)
    {
        return Ok(await _estoqueService.RegistrarSaidaAsync(request));
    }

    [HttpPost("ajuste")]
    public async Task<IActionResult> Ajustar([FromBody] RequestMovimentacaoEstoqueJson request)
    {
        return Ok(await _estoqueService.AjustarAsync(request));
    }

    [HttpGet("movimentacoes")]
    public async Task<IActionResult> GetMovimentacoes([FromQuery] Guid? produtoId)
    {
        return Ok(await _estoqueService.GetMovimentacoesAsync(produtoId));
    }
}
