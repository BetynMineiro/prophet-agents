using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Services.EntityId;
using Prophet.Application.UserCases.Pipeline.PipelineExecution;
using Prophet.CrossCutting.ResultObjects;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet;

public class CreateArtifactVersionUseCaseTests
{
    private readonly Mock<IPipelineExecutionStore> _storeMock;
    private readonly Mock<IEntityIdGenerator> _idGeneratorMock;
    private readonly CreateArtifactVersionUseCase _useCase;

    public CreateArtifactVersionUseCaseTests()
    {
        _storeMock = new Mock<IPipelineExecutionStore>();
        _idGeneratorMock = new Mock<IEntityIdGenerator>();
        _idGeneratorMock.Setup(x => x.NewId()).Returns(Guid.NewGuid());
        _storeMock.Setup(x => x.GetMaxVersionNumberAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _storeMock.Setup(x => x.AddVersionAsync(It.IsAny<ArtifactVersion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtifactVersion v, CancellationToken _) => v);
        _useCase = new CreateArtifactVersionUseCase(_storeMock.Object, _idGeneratorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProjectDoesNotExist_ReturnsNull()
    {
        var projectId = Guid.NewGuid();
        _storeMock.Setup(x => x.ProjectExistsActiveAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateArtifactVersionRequest(null, "Initial");
        var result = await _useCase.ExecuteAsync(projectId, request, CancellationToken.None);

        Assert.Null(result);
        _storeMock.Verify(x => x.AddVersionAsync(It.IsAny<ArtifactVersion>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenParentVersionNotFound_ReturnsNull()
    {
        var projectId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        _storeMock.Setup(x => x.ProjectExistsActiveAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _storeMock.Setup(x => x.GetVersionForProjectAsync(projectId, parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtifactVersion?)null);

        var request = new CreateArtifactVersionRequest(parentId, "Refinement");
        var result = await _useCase.ExecuteAsync(projectId, request, CancellationToken.None);

        Assert.Null(result);
        _storeMock.Verify(x => x.AddVersionAsync(It.IsAny<ArtifactVersion>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesVersionWithCorrectFields()
    {
        var projectId = Guid.NewGuid();
        _storeMock.Setup(x => x.ProjectExistsActiveAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _storeMock.Setup(x => x.GetMaxVersionNumberAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var request = new CreateArtifactVersionRequest(null, "  My change  ");
        var result = await _useCase.ExecuteAsync(projectId, request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(projectId, result.PipelineProjectId);
        Assert.Equal(3, result.VersionNumber);
        Assert.Equal("My change", result.ChangeSummary);
        Assert.Equal("Idle", result.PipelineStatus);
        Assert.Equal(0, result.CurrentStepIndex);
        _storeMock.Verify(x => x.AddVersionAsync(
            It.Is<ArtifactVersion>(v =>
                v.PipelineProjectId == projectId &&
                v.VersionNumber == 3 &&
                v.ChangeSummary == "My change" &&
                v.PipelineStatus == PipelineRunStatus.Idle),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithBlankChangeSummary_StoresNullChangeSummary()
    {
        var projectId = Guid.NewGuid();
        _storeMock.Setup(x => x.ProjectExistsActiveAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _storeMock.Setup(x => x.GetMaxVersionNumberAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var request = new CreateArtifactVersionRequest(null, "   ");
        var result = await _useCase.ExecuteAsync(projectId, request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.ChangeSummary);
        _storeMock.Verify(x => x.AddVersionAsync(
            It.Is<ArtifactVersion>(v => v.ChangeSummary == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParentVersion_CreatesVersionLinkedToParent()
    {
        var projectId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var parentVersion = new ArtifactVersion
        {
            Id = parentId,
            PipelineProjectId = projectId,
            VersionNumber = 1,
            PipelineStatus = PipelineRunStatus.Completed
        };
        _storeMock.Setup(x => x.ProjectExistsActiveAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _storeMock.Setup(x => x.GetVersionForProjectAsync(projectId, parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentVersion);
        _storeMock.Setup(x => x.GetMaxVersionNumberAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new CreateArtifactVersionRequest(parentId, "Refinement");
        var result = await _useCase.ExecuteAsync(projectId, request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(parentId, result.ParentVersionId);
        Assert.Equal(2, result.VersionNumber);
    }
}
