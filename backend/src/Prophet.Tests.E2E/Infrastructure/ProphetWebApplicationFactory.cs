using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Prophet.Tests.E2E.Infrastructure;

public class ProphetWebApplicationFactory : WebApplicationFactory<Program>
{
    private Dictionary<string, string?> TestConfig => new()
    {
        ["RateLimit:Api:PermitLimit"] = "1000",
        ["RateLimit:Api:WindowSeconds"] = "60",
        ["Storage:ApiBaseUrl"] = "http://localhost",
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(TestConfig));
        return base.CreateHost(builder);
    }
}
