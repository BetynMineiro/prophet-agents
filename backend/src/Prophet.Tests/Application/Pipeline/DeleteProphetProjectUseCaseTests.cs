using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline;
using Prophet.Application.UserCases.Pipeline.Projects;
using Prophet.Application.UserCases.Pipeline.Validators;
using Prophet.CrossCutting.Validation;

namespace Prophet.Tests.Application.Prophet;

public class DeletePipelineProjectUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _storeMock;
    private readonly IValidationErrorCollector _errorCollector;
    private readonly DeletePipelineProjectUseCase _useCase;

    public DeletePipelineProjectUseCaseTests()
    {
        _storeMock = new Mock<IPipelineProjectStore>();
        _errorCollector = new ValidationErrorCollector();
        var validator = new PipelineProjectIdQueryValidator();
        _useCase = new DeletePipelineProjectUseCase(_storeMock.Object, validator, _errorCollector);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreReturnsFalse_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        _storeMock.Setup(x => x.SoftDeleteAsync(id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _useCase.ExecuteAsync(id, CancellationToken.None);

        Assert.False(result);
        Assert.False(_errorCollector.HasErrors);
        _storeMock.Verify(x => x.SoftDeleteAsync(id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreReturnsTrue_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _storeMock.Setup(x => x.SoftDeleteAsync(id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _useCase.ExecuteAsync(id, CancellationToken.None);

        Assert.True(result);
        Assert.False(_errorCollector.HasErrors);
        _storeMock.Verify(x => x.SoftDeleteAsync(id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyId_AddsError_ReturnsFalse_DoesNotCallStore()
    {
        var result = await _useCase.ExecuteAsync(Guid.Empty, CancellationToken.None);

        Assert.False(result);
        Assert.True(_errorCollector.HasErrors);
        _storeMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
