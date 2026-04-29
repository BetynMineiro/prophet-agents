using Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;
using Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs.Validators;

namespace Prophet.Tests.Application.Prophet.ProjectHtmlPocs.Validators;

public sealed class HtmlPocIdQueryValidatorTests
{
    private readonly PipelineHtmlPocIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenBothEmpty_ReturnsBothErrors()
    {
        var r = _validator.Validate(new PipelineHtmlPocIdQuery(Guid.Empty, Guid.Empty));
        Assert.False(r.IsValid);
        Assert.Contains("Project id is required.", r.Errors);
        Assert.Contains("Document id is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenOnlyProjectIdEmpty_ReturnsProjectError()
    {
        var r = _validator.Validate(new PipelineHtmlPocIdQuery(Guid.Empty, Guid.NewGuid()));
        Assert.False(r.IsValid);
        Assert.Contains("Project id is required.", r.Errors);
        Assert.DoesNotContain("Document id is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenBothValid_ReturnsOk()
    {
        var r = _validator.Validate(new PipelineHtmlPocIdQuery(Guid.NewGuid(), Guid.NewGuid()));
        Assert.True(r.IsValid);
        Assert.Empty(r.Errors);
    }
}
