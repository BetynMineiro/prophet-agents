using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.Application.UserCases.Pipeline.ProjectInputs.Validators;

namespace Prophet.Tests.Application.Prophet.ProjectInputs.Validators;

public sealed class InputDocumentIdQueryValidatorTests
{
    private readonly PipelineInputDocumentIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenBothEmpty_ReturnsBothErrors()
    {
        var r = _validator.Validate(new PipelineInputDocumentIdQuery(Guid.Empty, Guid.Empty));
        Assert.False(r.IsValid);
        Assert.Contains("Project id is required.", r.Errors);
        Assert.Contains("Document id is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenOnlyDocumentIdEmpty_ReturnsDocumentError()
    {
        var r = _validator.Validate(new PipelineInputDocumentIdQuery(Guid.NewGuid(), Guid.Empty));
        Assert.False(r.IsValid);
        Assert.Contains("Document id is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenBothValid_ReturnsOk()
    {
        var r = _validator.Validate(new PipelineInputDocumentIdQuery(Guid.NewGuid(), Guid.NewGuid()));
        Assert.True(r.IsValid);
        Assert.Empty(r.Errors);
    }
}
