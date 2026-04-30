using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using Prophet.CrossCutting.Middleware;
using Prophet.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddHttpContextAccessor();
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

app.UseMiddleware<TraceIdLoggingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors("DefaultCors");
app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapControllers();

await app.RunAsync();

#pragma warning disable ASP0027
public partial class Program
{
    protected Program() { }
}
#pragma warning restore ASP0027
