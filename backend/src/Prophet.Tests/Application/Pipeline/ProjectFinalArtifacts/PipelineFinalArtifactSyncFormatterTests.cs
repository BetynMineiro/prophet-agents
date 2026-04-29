using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

namespace Prophet.Tests.Application.Pipeline.ProjectFinalArtifacts;

public sealed class PipelineFinalArtifactSyncFormatterTests
{
    [Fact]
    public void BuildDiagramMarkdown_UsesMermaidFenceWhenPropertyPresent()
    {
        const string json = """{"mermaid":"classDiagram\nclass A"}""";
        var md = PipelineFinalArtifactSyncFormatter.BuildDiagramMarkdown("Class diagram", "version 3", json);
        Assert.Contains("```mermaid", md, StringComparison.Ordinal);
        Assert.Contains("classDiagram", md, StringComparison.Ordinal);
        Assert.Contains("version 3", md, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildArchitectureMarkdown_EmbedsPrettyJson()
    {
        const string json = """{"style":"modular","layers":[]}""";
        var md = PipelineFinalArtifactSyncFormatter.BuildArchitectureMarkdown("version 1", json);
        Assert.Contains("# Architecture", md, StringComparison.Ordinal);
        Assert.Contains("```json", md, StringComparison.Ordinal);
        Assert.Contains("modular", md, StringComparison.Ordinal);
    }
}
