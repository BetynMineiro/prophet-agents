using Prophet.Application.UserCases.Pipeline.Projects;
using Prophet.Application.UserCases.Pipeline.Projects.Validators;

namespace Prophet.Tests.Application.Prophet.Projects.Validators;

public sealed class UpdateProphetProjectRequestValidatorTests
{
    private readonly UpdatePipelineProjectRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenNull_ReturnsInvalid()
    {
        var r = _validator.Validate(null);
        Assert.False(r.IsValid);
        Assert.Contains("Request is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ReturnsInvalid()
    {
        var r = _validator.Validate(new UpdatePipelineProjectRequest("", null, null, true));
        Assert.False(r.IsValid);
        Assert.Contains("Name is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenNameExceeds256_ReturnsInvalid()
    {
        var longName = new string('a', 257);
        var r = _validator.Validate(new UpdatePipelineProjectRequest(longName, null, null, true));
        Assert.False(r.IsValid);
        Assert.Contains("Name must be at most 256 characters.", r.Errors);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds4096_ReturnsInvalid()
    {
        var longDesc = new string('x', 4097);
        var r = _validator.Validate(new UpdatePipelineProjectRequest("Valid Name", longDesc, null, true));
        Assert.False(r.IsValid);
        Assert.Contains("Description must be at most 4096 characters.", r.Errors);
    }

    [Fact]
    public void Validate_WhenValid_ReturnsOk()
    {
        var r = _validator.Validate(new UpdatePipelineProjectRequest("Updated Name", "New description.", null, true));
        Assert.True(r.IsValid);
        Assert.Empty(r.Errors);
    }

    [Fact]
    public void Validate_WhenNameExactly256_ReturnsOk()
    {
        var name = new string('a', 256);
        var r = _validator.Validate(new UpdatePipelineProjectRequest(name, null, null, false));
        Assert.True(r.IsValid);
    }
}
