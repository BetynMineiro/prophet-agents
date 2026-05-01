using Microsoft.Extensions.Options;
using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.Options;
using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.Application.UserCases.Pipeline.ProjectInputs.Validators;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet.ProjectInputs;

public sealed class UploadPipelineInputDocumentsUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _projectStore = new();
    private readonly Mock<IPipelineInputDocumentStore> _documentStore = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly UploadPipelineInputDocumentsRequestValidator _validator = new();
    private readonly ValidationErrorCollector _errorCollector = new();
    private readonly IOptions<StorageOptions> _storageOptions = Options.Create(new StorageOptions
    {
        Root = "prophet",
        ApiBaseUrl = "https://localhost:7017",
    });

    private UploadPipelineInputDocumentsUseCase CreateSut() =>
        new(
            _projectStore.Object,
            _documentStore.Object,
            _storage.Object,
            _storageOptions,
            _validator,
            _errorCollector);

    [Fact]
    public async Task ExecuteAsync_WhenValidationFails_ReturnsNullAndCollectsErrors()
    {
        _errorCollector.Clear();
        var sut = CreateSut();
        var projectId = Guid.NewGuid();

        var result = await sut.ExecuteAsync(projectId, [], CancellationToken.None);

        Assert.Null(result);
        Assert.True(_errorCollector.HasErrors);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProjectMissing_ReturnsNull()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineProject?)null);
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, [new InputFileChunk("a.txt", "text/plain", [1])], CancellationToken.None);

        Assert.Null(result);
        Assert.False(_errorCollector.HasErrors);
        _storage.Verify(
            x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSkipReason_SkipsUploadAndReturnsFailureItem()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineProject { Id = projectId, Name = "P" });
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(
            projectId,
            [new InputFileChunk("a.txt", "text/plain", [], $"File exceeds {UploadPipelineInputDocumentsUseCase.MaxFileBytes / (1024 * 1024)} MB limit.")],
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result!.Results);
        Assert.False(result.Results[0].Success);
        Assert.Equal($"File exceeds {UploadPipelineInputDocumentsUseCase.MaxFileBytes / (1024 * 1024)} MB limit.", result.Results[0].ErrorMessage);
        _storage.Verify(
            x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmptyContent_ReturnsFailureItem()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineProject { Id = projectId, Name = "P" });
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(
            projectId,
            [new InputFileChunk("empty.txt", "text/plain", [])],
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result!.Results);
        Assert.False(result.Results[0].Success);
        Assert.Equal("File is empty.", result.Results[0].ErrorMessage);
        _storage.Verify(
            x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUploadSucceeds_PersistsDocumentAndReturnsDto()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineProject { Id = projectId, Name = "P" });
        _storage.Setup(x => x.UploadAsync(
                "genesis",
                "prophet",
                projectId.ToString("D"),
                "inputs",
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                "text/plain",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("genesis/prophet/x/inputs/y.txt");
        _documentStore.Setup(x => x.AddAsync(It.IsAny<PipelineInputDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineInputDocument e, CancellationToken _) =>
            {
                e.CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return e;
            });
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(
            projectId,
            [new InputFileChunk("note.txt", "text/plain", [1, 2, 3])],
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result!.Results);
        Assert.True(result.Results[0].Success);
        Assert.NotNull(result.Results[0].Document);
        Assert.Equal("note.txt", result.Results[0].FileName);
        _documentStore.Verify(x => x.AddAsync(It.IsAny<PipelineInputDocument>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
