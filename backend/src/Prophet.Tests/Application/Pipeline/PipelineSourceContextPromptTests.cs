using Prophet.Application.AgentPipeline;

namespace Prophet.Tests.Application.Pipeline;

public sealed class PipelineSourceContextPromptTests
{
    [Fact]
    public void BuildAnchoringSection_JoinsChunkTexts_and_keepsHeading()
    {
        const string json = """
            {
              "sourceFileName": "a.txt",
              "sourceFileNames": ["a.txt"],
              "chunks": [
                { "index": 0, "language": "text", "text": "Alpha" },
                { "index": 1, "language": "text", "text": "Beta" }
              ]
            }
            """;

        var section = PipelineSourceContextPrompt.BuildAnchoringSection(json, maxChars: 10_000);

        Assert.Contains("Original source material", section, StringComparison.Ordinal);
        Assert.Contains("Alpha", section, StringComparison.Ordinal);
        Assert.Contains("Beta", section, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildAnchoringSection_Truncates_withMarker_whenOverBudget()
    {
        var longChunk = new string('x', 500);
        var json = $$"""
            {"chunks":[{"index":0,"language":"text","text":"{{longChunk}}"}]}
            """;

        var section = PipelineSourceContextPrompt.BuildAnchoringSection(json, maxChars: 120);

        Assert.Contains("[... source truncated for length ...]", section, StringComparison.Ordinal);
        Assert.InRange(section.Length, 100, 250);
    }

    [Fact]
    public void BuildAnchoringSection_InvalidJson_returnsEmpty()
    {
        Assert.Equal("", PipelineSourceContextPrompt.BuildAnchoringSection("{not json"));
    }

    [Fact]
    public void PrependSourceAnchoring_whenEmptySection_returnsBaseOnly()
    {
        var merged = PipelineSourceContextPrompt.PrependSourceAnchoring("BASE", null);
        Assert.Equal("BASE", merged);
    }

    [Fact]
    public void BuildAnchoringSection_WhenChunksPropertyMissing_returnsEmpty()
    {
        const string json = """{"sourceFileName":"x","sourceFileNames":["x"]}""";
        Assert.Equal("", PipelineSourceContextPrompt.BuildAnchoringSection(json));
    }

    [Fact]
    public void BuildAnchoringSection_WhenMaxCharsZero_returnsEmpty()
    {
        const string json = """{"chunks":[{"index":0,"language":"text","text":"Hi"}]}""";
        Assert.Equal("", PipelineSourceContextPrompt.BuildAnchoringSection(json, maxChars: 0));
    }

    [Fact]
    public void PrependSourceAnchoring_prependsSection_whenChunksValid()
    {
        const string json = """{"chunks":[{"index":0,"language":"text","text":"Line1"}]}""";
        var merged = PipelineSourceContextPrompt.PrependSourceAnchoring("INSTRUCTION", json, maxChars: 500);
        Assert.StartsWith("## Original source material", merged, StringComparison.Ordinal);
        Assert.Contains("Line1", merged, StringComparison.Ordinal);
        Assert.EndsWith("INSTRUCTION", merged, StringComparison.Ordinal);
    }
}
