using Microsoft.AspNetCore.Mvc;
using TesteDDD.Application.Services;
using TesteDDD.Communication.Requests;

namespace TesteDDD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotaFiscalController : ControllerBase
{
    private readonly INotaFiscalService _notaFiscalService;
    private readonly ILogger<NotaFiscalController> _logger;

    public NotaFiscalController(INotaFiscalService notaFiscalService, ILogger<NotaFiscalController> logger)
    {
        _notaFiscalService = notaFiscalService ?? throw new ArgumentNullException(nameof(notaFiscalService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Emite uma nova nota fiscal para uma venda
    /// </summary>
    [HttpPost("emitir")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> EmitirNotaFiscal([FromBody] RequestEmitirNotaFiscalJson request)
    {
        try
        {
            _logger.LogInformation("Iniciando emissão de nota fiscal para venda: {VendasId}", request.VendasId);
            
            var response = await _notaFiscalService.EmitirAsync(request);
            
            return CreatedAtAction(nameof(ObterNotaFiscal), new { id = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Erro de validação: {Mensagem}", ex.Message);
            return BadRequest(new { erro = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Erro na operação: {Mensagem}", ex.Message);
            return BadRequest(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao emitir nota fiscal: {Mensagem}", ex.Message);
            return StatusCode(500, new { erro = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém uma nota fiscal pelo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ObterNotaFiscal(Guid id)
    {
        try
        {
            var response = await _notaFiscalService.ObterPorIdAsync(id);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Nota fiscal não encontrada: {Mensagem}", ex.Message);
            return NotFound(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao obter nota fiscal: {Mensagem}", ex.Message);
            return StatusCode(500, new { erro = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Consulta o status de uma nota fiscal no Sefaz
    /// </summary>
    [HttpGet("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ConsultarStatus(Guid id)
    {
        try
        {
            _logger.LogInformation("Consultando status da nota fiscal: {Id}", id);
            
            var response = await _notaFiscalService.ConsultarStatusAsync(id);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Erro ao consultar status: {Mensagem}", ex.Message);
            return NotFound(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao consultar status: {Mensagem}", ex.Message);
            return StatusCode(500, new { erro = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cancela uma nota fiscal
    /// </summary>
    [HttpPost("{id}/cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CancelarNotaFiscal(Guid id, [FromBody] RequestCancelarNotaFiscalJson request)
    {
        try
        {
            _logger.LogInformation("Cancelando nota fiscal: {Id}", id);
            
            var response = await _notaFiscalService.CancelarAsync(id, request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Erro de validação: {Mensagem}", ex.Message);
            return BadRequest(new { erro = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Erro na operação: {Mensagem}", ex.Message);
            return BadRequest(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao cancelar nota fiscal: {Mensagem}", ex.Message);
            return StatusCode(500, new { erro = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista notas fiscais por status
    /// </summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ListarPorStatus(int status)
    {
        try
        {
            _logger.LogInformation("Listando notas fiscais com status: {Status}", status);
            
            var response = await _notaFiscalService.ListarPorStatusAsync(status);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao listar notas fiscais: {Mensagem}", ex.Message);
            return StatusCode(500, new { erro = "Erro interno do servidor" });
        }
    }
}
