using Prophet.Application.UserCases.Pipeline;
using Prophet.Application.UserCases.Pipeline.Validators;

namespace Prophet.Tests.Application.Prophet.Validators;

public sealed class ProphetProjectIdQueryValidatorTests
{
    private readonly PipelineProjectIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenProjectIdIsEmpty_ReturnsInvalid()
    {
        var r = _validator.Validate(new PipelineProjectIdQuery(Guid.Empty));
        Assert.False(r.IsValid);
        Assert.Contains("Project id is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenProjectIdIsValid_ReturnsOk()
    {
        var r = _validator.Validate(new PipelineProjectIdQuery(Guid.NewGuid()));
        Assert.True(r.IsValid);
        Assert.Empty(r.Errors);
    }
}
