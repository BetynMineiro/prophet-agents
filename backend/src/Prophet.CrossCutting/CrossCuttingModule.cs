using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.OpenTelemetry;
using Prophet.CrossCutting.Metrics;

namespace Prophet.CrossCutting;

public static class CrossCuttingModule
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Configures Serilog with OpenTelemetry sink (OTLP gRPC) for logs in tracing context.
        /// </summary>
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
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Protocol = OtlpProtocol.Grpc;
                    options.ResourceAttributes["service.name"] = applicationName;
                    options.ResourceAttributes["deployment.environment"] = environment.EnvironmentName;
                    // 5.13: use same OTLP endpoint as tracing/metrics (configure via env / appsettings)
                    var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                        options.Endpoint = otlpEndpoint.Trim().TrimEnd('/');
                })
                .CreateLogger();

            services.AddSingleton(Log.Logger);
        }

        /// <summary>
        /// Configures OpenTelemetry: distributed tracing (ASP.NET Core + HttpClient) and metrics (AspNetCore, Runtime, HttpClient).
        /// Exports via OTLP gRPC to a collector (Jaeger, Tempo, etc.).
        /// </summary>
        public void ConfigureMetrics()
        {
            var applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownApp";

            services.AddOpenTelemetry()
                .WithTracing(tracer =>
                {
                    tracer
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService(serviceName: applicationName))
                        .AddOtlpExporter(otlp => { otlp.Protocol = OtlpExportProtocol.Grpc; });
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddMeter(MetricsMeterNames.Business) // 5.12: business metrics (auth via IMetrics)
                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService(applicationName))
                        .AddOtlpExporter(otlp => { otlp.Protocol = OtlpExportProtocol.Grpc; });
                });
        }

        /// <summary>
        /// Registers <see cref="IMetrics"/> (generic metrics) for use by domain adapters (e.g. IAuthMetrics implementation in Application).
        /// Call from composition root (e.g. ApiModule) after ConfigureMetrics.
        /// </summary>
        public void AddMetricsServices()
        {
            services.AddSingleton<IMetrics, MetricsService>();
        }
    }
}
