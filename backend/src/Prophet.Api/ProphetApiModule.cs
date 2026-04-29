using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prophet.Adapters.Llm;
using Prophet.Adapters.LocalStorage;
using Prophet.Adapters.Postgres;
using Prophet.Adapters.Postgres.Pipeline;
using Prophet.Application;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Diagnostics;
using Prophet.CrossCutting;
using Prophet.CrossCutting.Configurations;
using Prophet.CrossCutting.Filters;
using Prophet.CrossCutting.ModelBinding;
using Prophet.Api.HostedServices;
using Prophet.Api.Services;

namespace Prophet.Api;

public static class ProphetApiModule
{
    public static void ConfigureFilters(this MvcOptions options)
    {
        options.Filters.Add<ValidationErrorsResultFilter>();
        options.Filters.Add(new ProducesAttribute("application/json"));
        options.ModelBinderProviders.Insert(0, new StringToEnumModelBinderProvider());
    }

    extension(IServiceCollection services)
    {
        public void ConfigureProphetApiServicesLayer(IConfiguration configuration, IHostEnvironment environment)
        {
            services.ConfigureLogging(environment);
            services.ConfigureMetrics();
            services.AddMetricsServices();

            var useInMemory = string.Equals(configuration["Testing:UseInMemoryDatabase"], "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Environment.GetEnvironmentVariable("Testing__UseInMemoryDatabase"), "true", StringComparison.OrdinalIgnoreCase);
            var conn = ProphetConnectionStrings.GetPostgreSql(configuration);
            var devInMemoryFallback = environment.IsDevelopment() && string.IsNullOrWhiteSpace(conn);

            if (useInMemory)
            {
                var dbName = configuration["Testing:InMemoryDatabaseName"]
                    ?? Environment.GetEnvironmentVariable("Testing__InMemoryDatabaseName")
                    ?? "Prophet_" + Guid.NewGuid().ToString("N")[..8];
                services.AddDbContext<ProphetDbContext>(options => options.UseInMemoryDatabase(dbName + "_prophet"));
            }
            else if (!string.IsNullOrWhiteSpace(conn))
            {
                var prophetMigrationsAssembly = typeof(ProphetDbContext).Assembly.GetName().Name!;
                services.AddDbContext<ProphetDbContext>(options =>
                    options.UseNpgsql(conn, npgsql =>
                    {
                        npgsql.MigrationsAssembly(prophetMigrationsAssembly);
                        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", ProphetSchema.Name);
                    }));
            }
            else if (devInMemoryFallback)
            {
                services.AddDbContext<ProphetDbContext>(options =>
                    options.UseInMemoryDatabase("Prophet_Dev_Local_prophet"));
            }
            else
            {
                throw new InvalidOperationException(
                    "Set ConnectionStrings:postgresdb or ConnectionStrings:Default, or run under Development with no connection string to use a local in-memory DB.");
            }

            services.AddPipelineApplication();
            services.AddLocalDiagnosticsApi(configuration);
            services.AddScoped<IDiagnosticsBroadcaster, SignalRDiagnosticsBroadcaster>();
            services.AddHostedService<DiagnosticsBroadcastHostedService>();
            services.AddLlmAdapter(configuration);
            services.AddLocalStorage(configuration);
            services.AddScoped<IPipelineProjectStore, PipelineProjectStore>();
            services.AddScoped<IPipelineInputDocumentStore, PipelineInputDocumentStore>();
            services.AddScoped<IPipelineFinalArtifactStore, PipelineFinalArtifactStore>();
            services.AddScoped<IPipelineHtmlPocStore, PipelineHtmlPocStore>();
            services.AddScoped<IPipelineExecutionStore, PipelineExecutionStore>();
        }
    }
}
