using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using TesteDDD.Infrastructure.Data;

namespace TesteDDD.Api.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"TesteDDDTests-{Guid.NewGuid()}";
    private readonly string _contentRoot = GetApiContentRoot();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_CONTENTROOT", _contentRoot);
        Directory.SetCurrentDirectory(_contentRoot);

        builder.UseEnvironment("Testing");
        builder.UseContentRoot(_contentRoot);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });
    }

    private static string GetApiContentRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, "src", "Backend", "TesteDDD.Api");
            if (Directory.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Nao foi possivel localizar o diretorio do projeto TesteDDD.Api.");
    }
}
