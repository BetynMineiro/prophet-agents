using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;
using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts.Validators;

namespace Prophet.Tests.Application.Prophet.ProjectFinalArtifacts.Validators;

public sealed class FinalArtifactIdQueryValidatorTests
{
    private readonly PipelineFinalArtifactIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenBothEmpty_ReturnsBothErrors()
    {
        var r = _validator.Validate(new PipelineFinalArtifactIdQuery(Guid.Empty, Guid.Empty));
        Assert.False(r.IsValid);
        Assert.Contains("Project id is required.", r.Errors);
        Assert.Contains("Document id is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenOnlyProjectIdEmpty_ReturnsProjectError()
    {
        var r = _validator.Validate(new PipelineFinalArtifactIdQuery(Guid.Empty, Guid.NewGuid()));
        Assert.False(r.IsValid);
        Assert.Contains("Project id is required.", r.Errors);
        Assert.DoesNotContain("Document id is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenOnlyDocumentIdEmpty_ReturnsDocumentError()
    {
        var r = _validator.Validate(new PipelineFinalArtifactIdQuery(Guid.NewGuid(), Guid.Empty));
        Assert.False(r.IsValid);
        Assert.Contains("Document id is required.", r.Errors);
        Assert.DoesNotContain("Project id is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenBothValid_ReturnsOk()
    {
        var r = _validator.Validate(new PipelineFinalArtifactIdQuery(Guid.NewGuid(), Guid.NewGuid()));
        Assert.True(r.IsValid);
        Assert.Empty(r.Errors);
    }
}
