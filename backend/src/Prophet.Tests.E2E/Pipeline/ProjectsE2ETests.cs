using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Prophet.Tests.E2E.Infrastructure;

namespace Prophet.Tests.E2E.Pipeline;

public class ProjectsE2ETests : IClassFixture<ProphetWebApplicationFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ProjectsE2ETests(ProphetWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object CreateProjectBody(string name = "Test Project", string? description = "Test description", DateOnly? expectedDate = null)
        => new { name, description, expectedDate };

    [Fact]
    public async Task PostProject_WithValidData_Returns201_WithCorrectFields()
    {
        var body = CreateProjectBody("My Pipeline Project", "A test pipeline project");

        var response = await _client.PostAsJsonAsync("/v1/prophet/projects", body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var data = root.GetProperty("data");
        Assert.Equal("My Pipeline Project", data.GetProperty("name").GetString());
        Assert.Equal("A test pipeline project", data.GetProperty("description").GetString());
        Assert.True(data.GetProperty("isActive").GetBoolean());
        Assert.NotEqual(Guid.Empty, data.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task PostProject_WithEmptyName_Returns400()
    {
        var body = new { name = "", description = "desc" };

        var response = await _client.PostAsJsonAsync("/v1/prophet/projects", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_Returns200_WithFlatList()
    {
        await _client.PostAsJsonAsync("/v1/prophet/projects", CreateProjectBody("List Test Project"));

        var response = await _client.GetAsync("/v1/prophet/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var data = root.GetProperty("data");
        Assert.Equal(JsonValueKind.Array, data.ValueKind);
        Assert.True(data.GetArrayLength() >= 1);
    }

    [Fact]
    public async Task GetProject_AfterCreate_Returns200_WithCorrectProject()
    {
        var createResponse = await _client.PostAsJsonAsync("/v1/prophet/projects", CreateProjectBody("Get Me"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createJson = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var id = createDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var getResponse = await _client.GetAsync($"/v1/prophet/projects/{id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getJson = await getResponse.Content.ReadAsStringAsync();
        using var getDoc = JsonDocument.Parse(getJson);
        var data = getDoc.RootElement.GetProperty("data");
        Assert.Equal(id, data.GetProperty("id").GetGuid());
        Assert.Equal("Get Me", data.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetProject_UnknownId_Returns404()
    {
        var unknownId = Guid.NewGuid();
        var response = await _client.GetAsync($"/v1/prophet/projects/{unknownId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutProject_Returns200_WithUpdatedFields()
    {
        var createResponse = await _client.PostAsJsonAsync("/v1/prophet/projects", CreateProjectBody("Original Name"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createJson = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var id = createDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var updateBody = new { name = "Updated Name", description = "Updated description", expectedDate = (DateOnly?)null, isActive = true };
        var updateResponse = await _client.PutAsJsonAsync($"/v1/prophet/projects/{id}", updateBody);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updateJson = await updateResponse.Content.ReadAsStringAsync();
        using var updateDoc = JsonDocument.Parse(updateJson);
        var data = updateDoc.RootElement.GetProperty("data");
        Assert.Equal("Updated Name", data.GetProperty("name").GetString());
        Assert.Equal("Updated description", data.GetProperty("description").GetString());
    }

    [Fact]
    public async Task DeleteProject_Returns200_SoftDeletes()
    {
        var createResponse = await _client.PostAsJsonAsync("/v1/prophet/projects", CreateProjectBody("Delete Me"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createJson = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var id = createDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var deleteResponse = await _client.DeleteAsync($"/v1/prophet/projects/{id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var deleteJson = await deleteResponse.Content.ReadAsStringAsync();
        using var deleteDoc = JsonDocument.Parse(deleteJson);
        Assert.True(deleteDoc.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task RestoreProject_AfterDelete_Returns200()
    {
        var createResponse = await _client.PostAsJsonAsync("/v1/prophet/projects", CreateProjectBody("Restore Me"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createJson = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var id = createDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        await _client.DeleteAsync($"/v1/prophet/projects/{id}");

        var restoreResponse = await _client.PatchAsync($"/v1/prophet/projects/{id}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        var restoreJson = await restoreResponse.Content.ReadAsStringAsync();
        using var restoreDoc = JsonDocument.Parse(restoreJson);
        var data = restoreDoc.RootElement.GetProperty("data");
        Assert.True(data.GetProperty("isActive").GetBoolean());
    }
}
