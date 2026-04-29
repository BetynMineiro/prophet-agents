using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Domain.Pipeline;

public sealed class PipelineTextInputFileRulesTests
{
    [Theory]
    [InlineData("readme.md", true)]
    [InlineData("doc.TXT", true)]
    [InlineData("data.json", true)]
    [InlineData("schema.xml", true)]
    [InlineData("x.yml", true)]
    [InlineData("no-ext", false)]
    [InlineData("report.pdf", true)]
    [InlineData("spec.DOCX", true)]
    [InlineData("legacy.doc", true)]
    [InlineData("archive.zip", false)]
    public void IsAllowedExtension_respects_list(string name, bool expected) =>
        Assert.Equal(expected, PipelineTextInputFileRules.IsAllowedExtension(name));
}
