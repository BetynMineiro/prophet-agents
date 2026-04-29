using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;
using Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs.Validators;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet.ProjectHtmlPocs;

public sealed class GetPipelineHtmlPocContentUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _projectStore = new();
    private readonly Mock<IPipelineHtmlPocStore> _pocStore = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly PipelineHtmlPocIdQueryValidator _validator = new();
    private readonly ValidationErrorCollector _errorCollector = new();

    private GetPipelineHtmlPocContentUseCase CreateSut() =>
        new(_projectStore.Object, _pocStore.Object, _storage.Object, _validator, _errorCollector);

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
    public async Task ExecuteAsync_WhenBytesAvailable_ReturnsUtf8Html()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineProject { Id = projectId, Name = "P" });
        _pocStore.Setup(x => x.GetByIdAsync(projectId, documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineHtmlPoc
            {
                Id = documentId,
                PipelineProjectId = projectId,
                StorageObjectPath = "genesis/p/x/html-pocs/a.html",
            });
        _storage.Setup(x => x.ReadObjectAsync("genesis/p/x/html-pocs/a.html", It.IsAny<CancellationToken>()))
            .ReturnsAsync("<html></html>"u8.ToArray());
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, documentId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("<html></html>", result!.Text);
    }
}
