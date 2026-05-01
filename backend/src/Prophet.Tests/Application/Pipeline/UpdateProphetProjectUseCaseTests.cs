using System.Collections.Generic;
using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline.Projects;
using Prophet.Application.UserCases.Pipeline.Projects.Validators;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet;

public class UpdatePipelineProjectUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _storeMock;
    private readonly IValidationErrorCollector _errorCollector;
    private readonly UpdatePipelineProjectUseCase _useCase;

    public UpdatePipelineProjectUseCaseTests()
    {
        _storeMock = new Mock<IPipelineProjectStore>();
        _storeMock.Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());
        _errorCollector = new ValidationErrorCollector();
        var validator = new UpdatePipelineProjectRequestValidator();
        _useCase = new UpdatePipelineProjectUseCase(_storeMock.Object, validator, _errorCollector);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNameEmpty_AddsError_ReturnsNull()
    {
        var id = Guid.NewGuid();
        var result = await _useCase.ExecuteAsync(id, new UpdatePipelineProjectRequest("", null, null, true));

        Assert.Null(result);
        Assert.Contains(_errorCollector.GetErrors(), e => e.Contains("Name", StringComparison.OrdinalIgnoreCase));
        _storeMock.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<DateOnly?>(), It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _storeMock.Setup(x => x.UpdateAsync(id, It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<DateOnly?>(), It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineProject?)null);

        var result = await _useCase.ExecuteAsync(id, new UpdatePipelineProjectRequest("Name", "Desc", null, true));

        Assert.Null(result);
        Assert.False(_errorCollector.HasErrors);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOk_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var updated = new PipelineProject
        {
            Id = id,
            Name = "N",
            Description = "D",
            CreatedAtUtc = DateTime.UtcNow
        };
        _storeMock.Setup(x => x.UpdateAsync(id, "N", "D", It.IsAny<DateOnly?>(), It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _useCase.ExecuteAsync(id, new UpdatePipelineProjectRequest("N", "D", null, true));

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("N", result.Name);
        Assert.True(result.IsActive);
    }
}
