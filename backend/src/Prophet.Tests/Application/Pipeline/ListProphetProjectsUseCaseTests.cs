using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline.Projects;
using Prophet.CrossCutting.RequestObjects;
using Prophet.CrossCutting.ResultObjects;
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

    [Fact]
    public async Task ExecuteAsync_ReturnsMappedItems_WithNoLatestPipeline()
    {
        var project = MakeProject("Alpha");
        var page = new CursorPage<PipelineProject> { Items = [project], HasNext = false };

        _storeMock
            .Setup(x => x.GetPageAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());

        var result = await CreateUseCase().ExecuteAsync(new PagedRequest());

        Assert.Single(result.Items);
        Assert.Equal("Alpha", result.Items[0].Name);
        Assert.Null(result.Items[0].LatestArtifactVersionId);
        Assert.Null(result.Items[0].LatestPipelineStatus);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLatestPipelineExists_MapsStatusAndVersionId()
    {
        var project = MakeProject("Beta");
        var versionId = Guid.NewGuid();
        var page = new CursorPage<PipelineProject> { Items = [project], HasNext = false };

        _storeMock
            .Setup(x => x.GetPageAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>
            {
                [project.Id] = (versionId, PipelineRunStatus.Completed)
            });

        var result = await CreateUseCase().ExecuteAsync(new PagedRequest());

        Assert.Single(result.Items);
        Assert.Equal(versionId, result.Items[0].LatestArtifactVersionId);
        Assert.Equal("Completed", result.Items[0].LatestPipelineStatus);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDeletedProject_IsActiveIsFalse()
    {
        var project = MakeProject("Deleted", deleted: true);
        var page = new CursorPage<PipelineProject> { Items = [project], HasNext = false };

        _storeMock
            .Setup(x => x.GetPageAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());

        var result = await CreateUseCase().ExecuteAsync(new PagedRequest());

        Assert.Single(result.Items);
        Assert.False(result.Items[0].IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_ClampsPageSizeTo1_WhenRequestedZero()
    {
        var page = new CursorPage<PipelineProject> { Items = [] };

        _storeMock
            .Setup(x => x.GetPageAsync(1, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());

        await CreateUseCase().ExecuteAsync(new PagedRequest { PageSize = 0 });

        _storeMock.Verify(x => x.GetPageAsync(1, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ClampsPageSizeTo500_WhenRequestedTooLarge()
    {
        var page = new CursorPage<PipelineProject> { Items = [] };

        _storeMock
            .Setup(x => x.GetPageAsync(500, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());

        await CreateUseCase().ExecuteAsync(new PagedRequest { PageSize = 9999 });

        _storeMock.Verify(x => x.GetPageAsync(500, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCursorAndSearchTextToStore()
    {
        var page = new CursorPage<PipelineProject> { Items = [] };
        var request = new PagedRequest { PageSize = 10, Cursor = "cur-1", SearchText = "search", ActiveState = ActiveState.Active };

        _storeMock
            .Setup(x => x.GetPageAsync(10, "cur-1", "search", ActiveState.Active, It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());

        await CreateUseCase().ExecuteAsync(request);

        _storeMock.Verify(x => x.GetPageAsync(10, "cur-1", "search", ActiveState.Active, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesHasNextAndNextCursor()
    {
        var page = new CursorPage<PipelineProject> { Items = [], HasNext = true, NextCursor = "next-cursor" };

        _storeMock
            .Setup(x => x.GetPageAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());

        var result = await CreateUseCase().ExecuteAsync(new PagedRequest());

        Assert.True(result.HasNext);
        Assert.Equal("next-cursor", result.NextCursor);
    }

    [Fact]
    public async Task ExecuteAsync_PassesAllProjectIdsToLatestArtifactLookup()
    {
        var p1 = MakeProject("P1");
        var p2 = MakeProject("P2");
        var page = new CursorPage<PipelineProject> { Items = [p1, p2] };

        _storeMock
            .Setup(x => x.GetPageAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<ActiveState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());

        await CreateUseCase().ExecuteAsync(new PagedRequest());

        _storeMock.Verify(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(
            It.Is<IReadOnlyList<Guid>>(ids => ids.Contains(p1.Id) && ids.Contains(p2.Id) && ids.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
