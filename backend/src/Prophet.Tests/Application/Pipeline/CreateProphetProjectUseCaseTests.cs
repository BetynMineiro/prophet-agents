using Moq;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Services.EntityId;
using Prophet.Application.UserCases.Pipeline.Projects;
using Prophet.Application.UserCases.Pipeline.Projects.Validators;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Application.Prophet;

public class CreatePipelineProjectUseCaseTests
{
    private readonly Mock<IPipelineProjectStore> _storeMock;
    private readonly Mock<IEntityIdGenerator> _idGeneratorMock;
    private readonly Mock<IValidator<CreatePipelineProjectRequest>> _validatorMock;
    private readonly ValidationErrorCollector _errorCollector;
    private readonly CreatePipelineProjectUseCase _useCase;

    public CreatePipelineProjectUseCaseTests()
    {
        _storeMock = new Mock<IPipelineProjectStore>();
        _storeMock.Setup(x => x.CreateAsync(It.IsAny<PipelineProject>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineProject e, CancellationToken _) => e);
        _idGeneratorMock = new Mock<IEntityIdGenerator>();
        _idGeneratorMock.Setup(x => x.NewId()).Returns(Guid.NewGuid());
        _validatorMock = new Mock<IValidator<CreatePipelineProjectRequest>>();
        _validatorMock.Setup(x => x.Validate(It.IsAny<CreatePipelineProjectRequest>())).Returns(ValidationResult.Ok());
        _errorCollector = new ValidationErrorCollector();
        _useCase = new CreatePipelineProjectUseCase(_storeMock.Object, _idGeneratorMock.Object, _validatorMock.Object, _errorCollector);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidationFails_ReturnsNull_DoesNotCallStore()
    {
        _validatorMock.Setup(x => x.Validate(It.IsAny<CreatePipelineProjectRequest>())).Returns(ValidationResult.Fail(["Name is required."]));
        var request = new CreatePipelineProjectRequest("", null, null);
        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Null(result);
        Assert.True(_errorCollector.HasErrors);
        _storeMock.Verify(x => x.CreateAsync(It.IsAny<PipelineProject>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValid_CreatesEntity_ReturnsDto()
    {
        var request = new CreatePipelineProjectRequest("My project", "Desc", null);
        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("My project", result.Name);
        Assert.Equal("Desc", result.Description);
        Assert.True(result.IsActive);
        _storeMock.Verify(x => x.CreateAsync(It.Is<PipelineProject>(p => p.Name == "My project" && p.Description == "Desc"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
