using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Serilog;
using TesteDDD.Application.Services;
using TesteDDD.Application.Exceptions;
using TesteDDD.Application.Mappings;
using TesteDDD.Application.Validators;
using TesteDDD.Api.Middleware;
using TesteDDD.Domain.Repositories;
using TesteDDD.Infrastructure.Data;
using TesteDDD.Infrastructure.Repositories;
using TesteDDD.Infrastructure.Sefaz;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Registrar FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RequestProdutoJsonValidator>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' nao configurada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();

// Adicionar ICategoriaRepository no serviço de ProdutoService
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

builder.Services.AddScoped<IVendasRepository, VendasRepository>();
builder.Services.AddScoped<IVendasService, VendasService>();

builder.Services.AddScoped<IItemVendasRepository, ItemVendasRepository>();
builder.Services.AddScoped<IItemVendasService, ItemVendasService>();

builder.Services.AddScoped<INotaFiscalRepository, NotaFiscalRepository>();
builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>();

// Configurar Sefaz
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
    {
        dbContext.Database.Migrate();
    }
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var statusCode = exception is BusinessRuleException
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = exception is BusinessRuleException
                ? "Erro de regra de negocio."
                : "Ocorreu um erro interno.",
            Detail = app.Environment.IsDevelopment() || exception is BusinessRuleException
                ? exception?.Message
                : null,
            Instance = context.Request.Path
        };

        if (exception is BusinessRuleException businessRuleException)
        {
            problemDetails.Extensions["code"] = businessRuleException.Code;
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// Configure the HTTP request pipeline.
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
