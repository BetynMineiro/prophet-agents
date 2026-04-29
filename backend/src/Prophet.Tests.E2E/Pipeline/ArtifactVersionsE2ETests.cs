using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Prophet.Tests.E2E.Infrastructure;

namespace Prophet.Tests.E2E.Pipeline;

public class ArtifactVersionsE2ETests : IClassFixture<ProphetWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ArtifactVersionsE2ETests(ProphetWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateProjectAsync(string name = "Version Test Project")
    {
        var body = new { name, description = "desc for version tests" };
        var response = await _client.PostAsJsonAsync("/v1/prophet/projects", body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("data").GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task PostVersion_Returns200_WithVersionData()
    {
        var projectId = await CreateProjectAsync("Version Create Project");
        var body = new { changeSummary = "Initial version" };

        var response = await _client.PostAsJsonAsync($"/v1/prophet/projects/{projectId}/versions", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var data = root.GetProperty("data");
        Assert.Equal(projectId, data.GetProperty("pipelineProjectId").GetGuid());
        Assert.Equal(1, data.GetProperty("versionNumber").GetInt32());
        Assert.Equal("Initial version", data.GetProperty("changeSummary").GetString());
        Assert.Equal("Idle", data.GetProperty("pipelineStatus").GetString());
    }

    [Fact]
    public async Task GetVersions_Returns200_ContainsCreatedVersion()
    {
        var projectId = await CreateProjectAsync("Version List Project");
        var body = new { changeSummary = "Listed version" };
        await _client.PostAsJsonAsync($"/v1/prophet/projects/{projectId}/versions", body);

        var response = await _client.GetAsync($"/v1/prophet/projects/{projectId}/versions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var items = root.GetProperty("data").GetProperty("items");
        Assert.True(items.GetArrayLength() >= 1);
        Assert.Equal("Listed version", items[0].GetProperty("changeSummary").GetString());
    }

    [Fact]
    public async Task GetVersionById_Returns200_WithCorrectVersion()
    {
        var projectId = await CreateProjectAsync("Version GetById Project");
        var body = new { changeSummary = "Specific version" };
        var createResponse = await _client.PostAsJsonAsync($"/v1/prophet/projects/{projectId}/versions", body);

        var createJson = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var versionId = createDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var response = await _client.GetAsync($"/v1/prophet/projects/{projectId}/versions/{versionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(versionId, data.GetProperty("id").GetGuid());
        Assert.Equal("Specific version", data.GetProperty("changeSummary").GetString());
    }

    [Fact]
    public async Task GetVersions_ForUnknownProject_Returns404()
    {
        var unknownId = Guid.NewGuid();
        var response = await _client.GetAsync($"/v1/prophet/projects/{unknownId}/versions");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostVersion_ForUnknownProject_Returns404()
    {
        var unknownId = Guid.NewGuid();
        var body = new { changeSummary = "orphan version" };
        var response = await _client.PostAsJsonAsync($"/v1/prophet/projects/{unknownId}/versions", body);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
