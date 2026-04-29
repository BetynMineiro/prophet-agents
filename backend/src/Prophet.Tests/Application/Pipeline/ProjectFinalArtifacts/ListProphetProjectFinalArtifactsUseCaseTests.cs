using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline;
using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;
using Prophet.Application.UserCases.Pipeline.Validators;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet.ProjectFinalArtifacts;

public sealed class ListPipelineFinalArtifactsUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _projectStore = new();
    private readonly Mock<IPipelineFinalArtifactStore> _artifactStore = new();
    private readonly PipelineProjectIdQueryValidator _validator = new();
    private readonly ValidationErrorCollector _errorCollector = new();

    private ListPipelineFinalArtifactsUseCase CreateSut() =>
        new(_projectStore.Object, _artifactStore.Object, _validator, _errorCollector);

    [Fact]
    public async Task ExecuteAsync_WhenProjectMissing_ReturnsNull()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineProject?)null);
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, CancellationToken.None);

        Assert.Null(result);
        _artifactStore.Verify(x => x.ListByProjectAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_MapsEntitiesToItemDtos()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var created = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineProject { Id = projectId, Name = "P" });
        _artifactStore.Setup(x => x.ListByProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PipelineFinalArtifact>
            {
                new()
                {
                    Id = docId,
                    PipelineProjectId = projectId,
                    OriginalFileName = "report.md",
                    ContentType = "text/markdown",
                    SizeBytes = 42,
                    CreatedAtUtc = created,
                },
            });
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result!);
        var dto = result[0];
        Assert.Equal(docId, dto.Id);
        Assert.Equal("report.md", dto.OriginalFileName);
        Assert.Equal("text/markdown", dto.ContentType);
        Assert.Equal(42, dto.SizeBytes);
        Assert.Equal(created, dto.UploadedAtUtc);
    }
}
