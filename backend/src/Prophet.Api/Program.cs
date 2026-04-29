using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Prophet.Application.Middleware;
using Prophet.CrossCutting.Configurations;
using Prophet.CrossCutting.Middleware;
using Prophet.Api;
using Prophet.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
var healthBuilder = builder.Services.AddHealthChecks()
    .AddCheck("Liveness", _ => HealthCheckResult.Healthy(), tags: ["live"]);
var connString = ProphetConnectionStrings.GetPostgreSql(builder.Configuration);
if (!string.IsNullOrWhiteSpace(connString))
    healthBuilder.AddNpgSql(connString, name: "Database", tags: ["ready"]);

builder.Services.AddLogging();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("diagnostics-http");
builder.Services.AddSignalR();
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 52_428_800;
});
builder.Services.AddControllers(p => p.ConfigureFilters())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.ConfigureProphetApiServicesLayer(builder.Configuration, builder.Environment);
builder.Host.UseSerilog();

ProphetProgramSetup.AddRateLimiterPolicies(builder);
ProphetProgramSetup.AddCorsPolicy(builder);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });

var app = builder.Build();

ProphetProgramSetup.EnsureRequiredConfiguration(app);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapGet("/swagger", () => Results.Content(SwaggerUiPage.Html, "text/html")).ExcludeFromDescription();
}

app.UseMiddleware<TraceIdLoggingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors("DefaultCors");
app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseMiddleware<RequestMetricsMiddleware>();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapControllers();
app.MapHub<DiagnosticsHub>("/hubs/diagnostics");
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") });

await app.RunAsync();

#pragma warning disable ASP0027
public partial class Program
{
    protected Program() { }
}
#pragma warning restore ASP0027
