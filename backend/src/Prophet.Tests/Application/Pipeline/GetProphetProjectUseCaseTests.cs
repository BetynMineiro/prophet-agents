using System.Collections.Generic;
using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline;
using Prophet.Application.UserCases.Pipeline.Projects;
using Prophet.Application.UserCases.Pipeline.Validators;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet;

public class GetPipelineProjectUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _storeMock;
    private readonly IValidationErrorCollector _errorCollector;
    private readonly GetPipelineProjectUseCase _useCase;

    public GetPipelineProjectUseCaseTests()
    {
        _storeMock = new Mock<IPipelineProjectStore>();
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());
        _errorCollector = new ValidationErrorCollector();
        var validator = new PipelineProjectIdQueryValidator();
        _useCase = new GetPipelineProjectUseCase(_storeMock.Object, validator, _errorCollector);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreReturnsNull_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _storeMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineProject?)null);

        var result = await _useCase.ExecuteAsync(id, CancellationToken.None);

        Assert.Null(result);
        Assert.False(_errorCollector.HasErrors);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProjectFound_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var project = new PipelineProject
        {
            Id = id,
            Name = "Found Project",
            Description = "Some description",
            CreatedAtUtc = DateTime.UtcNow,
            DeletedAtUtc = null
        };
        _storeMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var result = await _useCase.ExecuteAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Found Project", result.Name);
        Assert.Equal("Some description", result.Description);
        Assert.True(result.IsActive);
        Assert.Null(result.LatestArtifactVersionId);
        Assert.Null(result.LatestPipelineStatus);
        _storeMock.Verify(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyId_AddsError_DoesNotCallStore()
    {
        var result = await _useCase.ExecuteAsync(Guid.Empty, CancellationToken.None);

        Assert.Null(result);
        Assert.True(_errorCollector.HasErrors);
        _storeMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLatestVersionExists_MapsStatusAndVersionId()
    {
        var id = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var project = new PipelineProject
        {
            Id = id,
            Name = "Project With Version",
            CreatedAtUtc = DateTime.UtcNow,
            DeletedAtUtc = null
        };
        _storeMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _storeMock
            .Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>
            {
                [id] = (versionId, PipelineRunStatus.Completed)
            });

        var result = await _useCase.ExecuteAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(versionId, result.LatestArtifactVersionId);
        Assert.Equal("Completed", result.LatestPipelineStatus);
    }
}
