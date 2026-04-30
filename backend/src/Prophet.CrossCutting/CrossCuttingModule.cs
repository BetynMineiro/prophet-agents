using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace Prophet.CrossCutting;

public static class CrossCuttingModule
{
    extension(IServiceCollection services)
    {
        /// <summary>Configures Serilog with a console sink only (no OTLP/OpenTelemetry).</summary>
        public void ConfigureLogging(IHostEnvironment environment)
        {
            var applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownApp";
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .MinimumLevel.Is(LogEventLevel.Information)
                .CreateLogger();

            services.AddSingleton(Log.Logger);
        }
    }
}
