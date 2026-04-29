using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline;
using Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;
using Prophet.Application.UserCases.Pipeline.Validators;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet.ProjectHtmlPocs;

public sealed class ListPipelineHtmlPocsUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _projectStore = new();
    private readonly Mock<IPipelineHtmlPocStore> _pocStore = new();
    private readonly PipelineProjectIdQueryValidator _validator = new();
    private readonly ValidationErrorCollector _errorCollector = new();

    private ListPipelineHtmlPocsUseCase CreateSut() =>
        new(_projectStore.Object, _pocStore.Object, _validator, _errorCollector);

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
        _pocStore.Verify(x => x.ListByProjectAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_MapsKindAndMetadata()
    {
        _errorCollector.Clear();
        var projectId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var created = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        _projectStore.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineProject { Id = projectId, Name = "P" });
        _pocStore.Setup(x => x.ListByProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PipelineHtmlPoc>
            {
                new()
                {
                    Id = docId,
                    PipelineProjectId = projectId,
                    PocKind = HtmlPocKind.Web,
                    OriginalFileName = "poc.html",
                    ContentType = "text/html",
                    SizeBytes = 100,
                    CreatedAtUtc = created,
                },
            });
        var sut = CreateSut();

        var result = await sut.ExecuteAsync(projectId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result!);
        var dto = result[0];
        Assert.Equal(docId, dto.Id);
        Assert.Equal(HtmlPocKind.Web, dto.Kind);
        Assert.Equal("poc.html", dto.OriginalFileName);
        Assert.Equal(100, dto.SizeBytes);
    }
}
