using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.Application.UserCases.Pipeline.ProjectInputs.Validators;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet.ProjectInputs;

public sealed class GetPipelineInputDownloadUrlUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _projectStore = new();
    private readonly Mock<IPipelineInputDocumentStore> _documentStore = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly PipelineInputDocumentIdQueryValidator _validator = new();
    private readonly ValidationErrorCollector _errorCollector = new();

    private GetPipelineInputDownloadUrlUseCase CreateSut() =>
        new(
            _projectStore.Object,
            _documentStore.Object,
            _storage.Object,
            _validator,
            _errorCollector);

    [Fact]
    public async Task ExecuteAsync_WhenProjectIdEmpty_ReturnsNull_CollectsErrors()
    {
        _errorCollector.Clear();
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
        Assert.True(_errorCollector.HasErrors);
        _storage.Verify(
            x => x.GetSignedUrlAsync(It.IsAny<string?>(), It.IsAny<TimeSpan>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDocumentIdEmpty_ReturnsNull_CollectsErrors()
    {
        _errorCollector.Clear();
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(Guid.NewGuid(), Guid.Empty, CancellationToken.None);

        Assert.Null(result);
        Assert.True(_errorCollector.HasErrors);
        _storage.Verify(
            x => x.GetSignedUrlAsync(It.IsAny<string?>(), It.IsAny<TimeSpan>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

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
        Assert.False(_errorCollector.HasErrors);
        _documentStore.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDocumentMissing_ReturnsNull()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineProject { Id = projectId, Name = "P" });
        _documentStore.Setup(x => x.GetByIdAsync(projectId, documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineInputDocument?)null);
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, documentId, CancellationToken.None);

        Assert.Null(result);
        _storage.Verify(
            x => x.GetSignedUrlAsync(It.IsAny<string?>(), It.IsAny<TimeSpan>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenFound_PassesOriginalFileName_ToGetSignedUrl_ReturnsDto()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        const string path = "genesis/prophet/x/inputs/doc.pdf";
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineProject { Id = projectId, Name = "P" });
        _documentStore.Setup(x => x.GetByIdAsync(projectId, documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineInputDocument
            {
                Id = documentId,
                PipelineProjectId = projectId,
                OriginalFileName = "report.pdf",
                StorageObjectPath = path,
            });
        _storage
            .Setup(x => x.GetSignedUrlAsync(path, It.IsAny<TimeSpan>(), "report.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://signed.example/get");
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, documentId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("https://signed.example/get", result!.DownloadUrl);
        _storage.Verify(
            x => x.GetSignedUrlAsync(path, TimeSpan.FromHours(1), "report.pdf", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGetSignedUrlReturnsNull_ReturnsEmptyDownloadUrl()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineProject { Id = projectId, Name = "P" });
        _documentStore.Setup(x => x.GetByIdAsync(projectId, documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineInputDocument
            {
                Id = documentId,
                PipelineProjectId = projectId,
                OriginalFileName = "a.txt",
                StorageObjectPath = "p/a",
            });
        _storage
            .Setup(x => x.GetSignedUrlAsync(It.IsAny<string?>(), It.IsAny<TimeSpan>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, documentId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("", result!.DownloadUrl);
    }
}
