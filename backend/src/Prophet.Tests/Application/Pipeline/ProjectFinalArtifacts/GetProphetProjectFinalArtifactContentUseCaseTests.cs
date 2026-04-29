using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;
using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts.Validators;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet.ProjectFinalArtifacts;

public sealed class GetPipelineFinalArtifactContentUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _projectStore = new();
    private readonly Mock<IPipelineFinalArtifactStore> _artifactStore = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly PipelineFinalArtifactIdQueryValidator _validator = new();
    private readonly ValidationErrorCollector _errorCollector = new();

    private GetPipelineFinalArtifactContentUseCase CreateSut() =>
        new(_projectStore.Object, _artifactStore.Object, _storage.Object, _validator, _errorCollector);

    [Fact]
    public async Task ExecuteAsync_WhenProjectMissing_ReturnsNull()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineProject?)null);
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, documentId, CancellationToken.None);

        Assert.Null(result);
        _storage.Verify(x => x.ReadObjectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBytesAvailable_ReturnsUtf8Text()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineProject { Id = projectId, Name = "P" });
        _artifactStore.Setup(x => x.GetByIdAsync(projectId, documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineFinalArtifact
            {
                Id = documentId,
                PipelineProjectId = projectId,
                StorageObjectPath = "genesis/p/a/f/x.md",
            });
        _storage.Setup(x => x.ReadObjectAsync("genesis/p/a/f/x.md", It.IsAny<CancellationToken>()))
            .ReturnsAsync("# Hi"u8.ToArray());
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, documentId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("# Hi", result!.Text);
    }
}
