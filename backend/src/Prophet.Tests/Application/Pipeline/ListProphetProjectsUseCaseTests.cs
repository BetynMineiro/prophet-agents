using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline.Projects;
using Prophet.CrossCutting.RequestObjects;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Pipeline;

public class ListProphetProjectsUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _storeMock = new();

    private ListPipelineProjectsUseCase CreateUseCase() => new(_storeMock.Object);

    private static PipelineProject MakeProject(string name, bool deleted = false) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Description = null,
        ExpectedDate = null,
        CreatedAtUtc = DateTime.UtcNow,
        DeletedAtUtc = deleted ? DateTime.UtcNow : null
    };

    private void SetupGetAll(IReadOnlyList<PipelineProject> projects)
    {
        _storeMock
            .Setup(x => x.GetAllAsync(It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMappedItems_WithNoLatestPipeline()
    {
        var project = MakeProject("Alpha");
        SetupGetAll([project]);

        var result = await CreateUseCase().ExecuteAsync(null, ActiveState.All);

        Assert.Single(result);
        Assert.Equal("Alpha", result[0].Name);
        Assert.Null(result[0].LatestArtifactVersionId);
        Assert.Null(result[0].LatestPipelineStatus);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLatestPipelineExists_MapsStatusAndVersionId()
    {
        var project = MakeProject("Beta");
        var versionId = Guid.NewGuid();
        _storeMock
            .Setup(x => x.GetAllAsync(It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([project]);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>
            {
                [project.Id] = (versionId, PipelineRunStatus.Completed)
            });

        var result = await CreateUseCase().ExecuteAsync(null, ActiveState.All);

        Assert.Single(result);
        Assert.Equal(versionId, result[0].LatestArtifactVersionId);
        Assert.Equal("Completed", result[0].LatestPipelineStatus);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDeletedProject_IsActiveIsFalse()
    {
        var project = MakeProject("Deleted", deleted: true);
        SetupGetAll([project]);

        var result = await CreateUseCase().ExecuteAsync(null, ActiveState.All);

        Assert.Single(result);
        Assert.False(result[0].IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_PassesSearchTextAndActiveStateToStore()
    {
        _storeMock
            .Setup(x => x.GetAllAsync("search", ActiveState.Active, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());

        await CreateUseCase().ExecuteAsync("search", ActiveState.Active);

        _storeMock.Verify(x => x.GetAllAsync("search", ActiveState.Active, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PassesAllProjectIdsToLatestArtifactLookup()
    {
        var p1 = MakeProject("P1");
        var p2 = MakeProject("P2");
        _storeMock
            .Setup(x => x.GetAllAsync(It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([p1, p2]);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());

        await CreateUseCase().ExecuteAsync(null, ActiveState.All);

        _storeMock.Verify(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(
            It.Is<IReadOnlyList<Guid>>(ids => ids.Contains(p1.Id) && ids.Contains(p2.Id) && ids.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenNoProjects()
    {
        SetupGetAll([]);

        var result = await CreateUseCase().ExecuteAsync(null, ActiveState.All);

        Assert.Empty(result);
    }
}
