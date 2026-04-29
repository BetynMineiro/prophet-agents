using Prophet.Tests.E2E.Infrastructure;

namespace Prophet.Tests.E2E.Pipeline;

public class HealthE2ETests : IClassFixture<ProphetWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthE2ETests(ProphetWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_Returns200()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealthReady_Returns200_WithInMemoryDatabase()
    {
        // /health/ready filters by "ready" tag — with in-memory DB no NpgSql check is registered,
        // so the endpoint returns 200 with an empty healthy result.
        var response = await _client.GetAsync("/health/ready");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
