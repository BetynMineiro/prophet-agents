using Prophet.Application.UserCases.Pipeline.Projects;
using Prophet.Application.UserCases.Pipeline.Projects.Validators;

namespace Prophet.Tests.Application.Prophet.Projects.Validators;

public sealed class CreateProphetProjectRequestValidatorTests
{
    private readonly CreatePipelineProjectRequestValidator _validator = new();

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
        var r = _validator.Validate(new CreatePipelineProjectRequest("", null, null));
        Assert.False(r.IsValid);
        Assert.Contains("Name is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenNameIsWhitespace_ReturnsInvalid()
    {
        var r = _validator.Validate(new CreatePipelineProjectRequest("   ", null, null));
        Assert.False(r.IsValid);
        Assert.Contains("Name is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenNameExceeds256_ReturnsInvalid()
    {
        var longName = new string('a', 257);
        var r = _validator.Validate(new CreatePipelineProjectRequest(longName, null, null));
        Assert.False(r.IsValid);
        Assert.Contains("Name must be at most 256 characters.", r.Errors);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds4096_ReturnsInvalid()
    {
        var longDesc = new string('x', 4097);
        var r = _validator.Validate(new CreatePipelineProjectRequest("Valid Name", longDesc, null));
        Assert.False(r.IsValid);
        Assert.Contains("Description must be at most 4096 characters.", r.Errors);
    }

    [Fact]
    public void Validate_WhenNameValidAndNoDescription_ReturnsOk()
    {
        var r = _validator.Validate(new CreatePipelineProjectRequest("My Project", null, null));
        Assert.True(r.IsValid);
        Assert.Empty(r.Errors);
    }

    [Fact]
    public void Validate_WhenBothNameAndDescriptionValid_ReturnsOk()
    {
        var r = _validator.Validate(new CreatePipelineProjectRequest("Project Alpha", "A description.", null));
        Assert.True(r.IsValid);
        Assert.Empty(r.Errors);
    }

    [Fact]
    public void Validate_WhenDescriptionExactly4096_ReturnsOk()
    {
        var desc = new string('y', 4096);
        var r = _validator.Validate(new CreatePipelineProjectRequest("Name", desc, null));
        Assert.True(r.IsValid);
    }
}
