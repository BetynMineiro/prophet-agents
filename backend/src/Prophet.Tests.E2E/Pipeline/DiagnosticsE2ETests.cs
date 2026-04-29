using System.Net;
using System.Text.Json;
using Prophet.Tests.E2E.Infrastructure;

namespace Prophet.Tests.E2E.Pipeline;

public class DiagnosticsE2ETests : IClassFixture<ProphetWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DiagnosticsE2ETests(ProphetWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDiagnosticsSummary_Returns200()
    {
        var response = await _client.GetAsync("/v1/prophet/diagnostics/summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.True(root.TryGetProperty("data", out _));
    }
}
