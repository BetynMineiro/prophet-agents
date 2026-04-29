using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline;
using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.Application.UserCases.Pipeline.Validators;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet.ProjectInputs;

public sealed class ListPipelineInputDocumentsUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _projectStore = new();
    private readonly Mock<IPipelineInputDocumentStore> _documentStore = new();
    private readonly PipelineProjectIdQueryValidator _validator = new();
    private readonly ValidationErrorCollector _errorCollector = new();

    private ListPipelineInputDocumentsUseCase CreateSut() =>
        new(_projectStore.Object, _documentStore.Object, _validator, _errorCollector);

    [Fact]
    public async Task ExecuteAsync_WhenProjectIdEmpty_ReturnsNull_CollectsErrors()
    {
        _errorCollector.Clear();
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(Guid.Empty, CancellationToken.None);

        Assert.Null(result);
        Assert.True(_errorCollector.HasErrors);
        _documentStore.Verify(x => x.ListByProjectAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

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
        _documentStore.Verify(x => x.ListByProjectAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoDocuments_ReturnsEmptyList()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineProject { Id = projectId, Name = "P" });
        _documentStore.Setup(x => x.ListByProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PipelineInputDocument>());
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result!);
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
        _documentStore.Setup(x => x.ListByProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PipelineInputDocument>
            {
                new()
                {
                    Id = docId,
                    PipelineProjectId = projectId,
                    OriginalFileName = "a.pdf",
                    ContentType = "application/pdf",
                    SizeBytes = 99,
                    CreatedAtUtc = created,
                },
            });
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result!);
        var dto = result[0];
        Assert.Equal(docId, dto.Id);
        Assert.Equal("a.pdf", dto.OriginalFileName);
        Assert.Equal("application/pdf", dto.ContentType);
        Assert.Equal(99, dto.SizeBytes);
        Assert.Equal(created, dto.UploadedAtUtc);
    }
}
