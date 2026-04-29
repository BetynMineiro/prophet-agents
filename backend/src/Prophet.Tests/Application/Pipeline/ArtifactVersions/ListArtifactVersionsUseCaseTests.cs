using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline.PipelineExecution;
using Prophet.CrossCutting.RequestObjects;
using Prophet.CrossCutting.ResultObjects;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet;

public class ListArtifactVersionsUseCaseTests
{
    private readonly Mock<IPipelineExecutionStore> _storeMock;
    private readonly ListArtifactVersionsUseCase _useCase;

    public ListArtifactVersionsUseCaseTests()
    {
        _storeMock = new Mock<IPipelineExecutionStore>();
        _useCase = new ListArtifactVersionsUseCase(_storeMock.Object);
    }

    private static ArtifactVersion MakeVersion(Guid projectId, int versionNumber = 1, string? changeSummary = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            PipelineProjectId = projectId,
            VersionNumber = versionNumber,
            ChangeSummary = changeSummary,
            PipelineStatus = PipelineRunStatus.Idle,
            CurrentStepIndex = 0,
            CreatedAtUtc = DateTime.UtcNow
        };

    [Fact]
    public async Task ExecuteAsync_WhenProjectNotFound_ReturnsNull()
    {
        var projectId = Guid.NewGuid();
        _storeMock.Setup(x => x.ProjectExistsActiveAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _useCase.ExecuteAsync(projectId, new PagedRequest(), CancellationToken.None);

        Assert.Null(result);
        _storeMock.Verify(x => x.ListVersionsPageAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProjectExists_ReturnsPage()
    {
        var projectId = Guid.NewGuid();
        var version = MakeVersion(projectId, 1, "First version");
        var page = new CursorPage<ArtifactVersion>
        {
            Items = [version],
            HasNext = false,
            NextCursor = null
        };

        _storeMock.Setup(x => x.ProjectExistsActiveAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _storeMock.Setup(x => x.ListVersionsPageAsync(projectId, It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _useCase.ExecuteAsync(projectId, new PagedRequest(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(version.Id, result.Items[0].Id);
        Assert.Equal("First version", result.Items[0].ChangeSummary);
        Assert.Equal("Idle", result.Items[0].PipelineStatus);
        Assert.False(result.HasNext);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesPageSizeAndCursorToStore()
    {
        var projectId = Guid.NewGuid();
        var page = new CursorPage<ArtifactVersion> { Items = [] };

        _storeMock.Setup(x => x.ProjectExistsActiveAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _storeMock.Setup(x => x.ListVersionsPageAsync(projectId, 50, "some-cursor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var request = new PagedRequest { PageSize = 50, Cursor = "some-cursor" };
        await _useCase.ExecuteAsync(projectId, request, CancellationToken.None);

        _storeMock.Verify(x => x.ListVersionsPageAsync(projectId, 50, "some-cursor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesHasNextAndNextCursor()
    {
        var projectId = Guid.NewGuid();
        var page = new CursorPage<ArtifactVersion>
        {
            Items = [],
            HasNext = true,
            NextCursor = "next-page-cursor"
        };

        _storeMock.Setup(x => x.ProjectExistsActiveAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _storeMock.Setup(x => x.ListVersionsPageAsync(projectId, It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _useCase.ExecuteAsync(projectId, new PagedRequest(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.HasNext);
        Assert.Equal("next-page-cursor", result.NextCursor);
    }

    [Fact]
    public async Task ExecuteAsync_MapsMultipleVersionsToDto()
    {
        var projectId = Guid.NewGuid();
        var v1 = MakeVersion(projectId, 1, "First");
        var v2 = MakeVersion(projectId, 2, "Second");
        var page = new CursorPage<ArtifactVersion> { Items = [v1, v2] };

        _storeMock.Setup(x => x.ProjectExistsActiveAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _storeMock.Setup(x => x.ListVersionsPageAsync(projectId, It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _useCase.ExecuteAsync(projectId, new PagedRequest(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(v1.Id, result.Items[0].Id);
        Assert.Equal(v2.Id, result.Items[1].Id);
        Assert.Equal("First", result.Items[0].ChangeSummary);
        Assert.Equal("Second", result.Items[1].ChangeSummary);
    }
}
