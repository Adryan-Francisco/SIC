using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TesteDDD.Api.Middleware;
using TesteDDD.Application.Exceptions;
using TesteDDD.Application.Mappings;
using TesteDDD.Application.Services;
using TesteDDD.Application.Validators;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;
using TesteDDD.Infrastructure.Repositories;
using TesteDDD.Infrastructure.Sefaz;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddValidatorsFromAssemblyContaining<RequestProdutoJsonValidator>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' nao configurada.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();

builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

builder.Services.AddScoped<IFornecedorRepository, FornecedorRepository>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();

builder.Services.AddScoped<IVendasRepository, VendasRepository>();
builder.Services.AddScoped<IVendasService, VendasService>();

builder.Services.AddScoped<IItemVendaRepository, ItemVendasRepository>();
builder.Services.AddScoped<IItemVendasService, ItemVendasService>();

builder.Services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
builder.Services.AddScoped<IOrdemServicoService, OrdemServicoService>();

builder.Services.AddScoped<IEstoqueRepository, EstoqueRepository>();
builder.Services.AddScoped<IMovimentacaoEstoqueRepository, MovimentacaoEstoqueRepository>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();

builder.Services.AddScoped<INotaFiscalRepository, NotaFiscalRepository>();
builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>();

var sefazSettings = new SefazIntegrationSettings
{
    UrlProducao = builder.Configuration["Sefaz:UrlProducao"],
    UrlHomologacao = builder.Configuration["Sefaz:UrlHomologacao"],
    UtilizarHomologacao = bool.Parse(builder.Configuration["Sefaz:UtilizarHomologacao"] ?? "true"),
    CertificadoCaminho = builder.Configuration["Sefaz:CertificadoCaminho"],
    CertificadoSenha = builder.Configuration["Sefaz:CertificadoSenha"],
    CnpjEmitente = builder.Configuration["Sefaz:CnpjEmitente"],
    RazaoSocial = builder.Configuration["Sefaz:RazaoSocial"],
    NomeFantasia = builder.Configuration["Sefaz:NomeFantasia"],
    Uf = builder.Configuration["Sefaz:Uf"],
    TimeoutSegundos = int.Parse(builder.Configuration["Sefaz:TimeoutSegundos"] ?? "30"),
    ConsultaStatusInterval = int.Parse(builder.Configuration["Sefaz:ConsultaStatusInterval"] ?? "2000"),
    ConsultaStatusMaxTentativas = int.Parse(builder.Configuration["Sefaz:ConsultaStatusMaxTentativas"] ?? "30"),
    Danfe = new()
    {
        CaminhoSalvamento = builder.Configuration["Danfe:CaminhoSalvamento"],
        Servidor = builder.Configuration["Danfe:Servidor"],
        CaminhoConsultaDanfe = builder.Configuration["Danfe:CaminhoConsultaDanfe"]
    }
};

builder.Services.AddSingleton(sefazSettings);
builder.Services.AddScoped<IXmlSignatureService, XmlSignatureService>();
builder.Services.AddScoped<IXmlGeracaoNfeService, XmlGeracaoNfeService>();
builder.Services.AddScoped<ISefazSoapClient, SefazSoapClient>();
builder.Services.AddScoped<IDanfePdfService, DanfePdfService>();
builder.Services.AddScoped<ISefazIntegrationService, SefazIntegrationService>();
builder.Services.AddHttpClient<ISefazIntegrationService, SefazIntegrationService>();
builder.Services.AddHttpClient<ISefazSoapClient, SefazSoapClient>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (dbContext.Database.IsRelational())
        dbContext.Database.Migrate();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception is BusinessRuleException businessRuleException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                status = StatusCodes.Status400BadRequest,
                title = "Erro de regra de negocio.",
                detail = businessRuleException.Message,
                instance = context.Request.Path.Value,
                code = businessRuleException.Code
            });
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Ocorreu um erro interno.",
            Detail = app.Environment.IsDevelopment() ? exception?.Message : null,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseValidationMiddleware();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
