using System.Collections.Generic;
using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline.Projects;
using Prophet.Application.UserCases.Pipeline.Validators;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet;

public class RestorePipelineProjectUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _storeMock;
    private readonly IValidationErrorCollector _errorCollector;
    private readonly RestorePipelineProjectUseCase _useCase;

    public RestorePipelineProjectUseCaseTests()
    {
        _storeMock = new Mock<IPipelineProjectStore>();
        _storeMock.Setup(x => x.GetLatestArtifactVersionPipelineByProjectIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>());
        _errorCollector = new ValidationErrorCollector();
        var idValidator = new PipelineProjectIdQueryValidator();
        _useCase = new RestorePipelineProjectUseCase(_storeMock.Object, idValidator, _errorCollector);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotDeleted_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _storeMock.Setup(x => x.RestoreAsync(id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineProject?)null);

        var result = await _useCase.ExecuteAsync(id, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOk_ReturnsDtoWithIsActiveTrue()
    {
        var id = Guid.NewGuid();
        var restored = new PipelineProject
        {
            Id = id,
            Name = "R",
            Description = null,
            CreatedAtUtc = DateTime.UtcNow,
            DeletedAtUtc = null
        };
        _storeMock.Setup(x => x.RestoreAsync(id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(restored);

        var result = await _useCase.ExecuteAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.True(result.IsActive);
    }
}
