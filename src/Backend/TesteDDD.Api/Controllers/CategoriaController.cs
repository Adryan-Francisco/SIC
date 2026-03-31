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
        return Created(string.Empty, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _categoriaService.GetAllAsync();
        return Ok(response);
    }

    // Endpoints de PUT e DELETE viriam aqui chamando o _productService...
}