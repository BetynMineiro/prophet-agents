using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Prophet.Adapters.Postgres.Persistence;

/// <summary>Design-time factory for Prophet PostgreSQL migrations (migrations live in this assembly).</summary>
public sealed class ProphetDbContextFactory : IDesignTimeDbContextFactory<ProphetDbContext>
{
    public ProphetDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = DesignTimeAppSettings.ResolveBasePath("Prophet.Api");
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("postgresdb") ?? config.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=genesis;Username=postgres;Password=postgres";

        var migrationsAssembly = typeof(ProphetDbContext).Assembly.GetName().Name!;

        var options = new DbContextOptionsBuilder<ProphetDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(migrationsAssembly);
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", ProphetSchema.Name);
            })
            .Options;

        return new ProphetDbContext(options);
    }
}
