using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Prophet.Tests.E2E.Infrastructure;

public class ProphetWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _inMemoryDatabaseName = "Prophet_E2E_" + Guid.NewGuid().ToString("N")[..8];

    private Dictionary<string, string?> TestConfig => new()
    {
        ["Testing:UseInMemoryDatabase"] = "true",
        ["Testing:InMemoryDatabaseName"] = _inMemoryDatabaseName,
        ["ConnectionStrings:Default"] = "",
        ["RateLimit:Api:PermitLimit"] = "1000",
        ["RateLimit:Api:WindowSeconds"] = "60",
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var e2eOutputDir = Path.GetDirectoryName(typeof(ProphetWebApplicationFactory).Assembly.Location)!;
            var path = Path.Combine(e2eOutputDir, "appsettings.Testing.json");
            config.AddJsonFile(path, optional: true);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(TestConfig));
        return base.CreateHost(builder);
    }
}
