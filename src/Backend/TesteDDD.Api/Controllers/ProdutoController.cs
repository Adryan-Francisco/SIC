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
        return Created(string.Empty, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _produtoService.GetAllAsync();
        return Ok(response);
    }

    // Endpoints de PUT e DELETE viriam aqui chamando o _productService...
}