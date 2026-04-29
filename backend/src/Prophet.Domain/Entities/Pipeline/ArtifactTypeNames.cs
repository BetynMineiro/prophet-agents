namespace Prophet.Domain.Entities.Pipeline;

/// <summary>Values for <see cref="PipelineArtifact.ArtifactType"/> (§5).</summary>
public static class ArtifactTypeNames
{
    public const string InputFile = "input-file";
    public const string Chunks = "chunks";
    public const string Insights = "insights";
    public const string MarketAnalysis = "market-analysis";
    public const string DomainModel = "domain-model";
    public const string Architecture = "architecture";
    public const string ClassDiagram = "class-diagram";
    public const string FlowDiagram = "flow-diagram";
    public const string PocWeb = "poc-web";
    public const string PocMobile = "poc-mobile";
    public const string Documentation = "documentation";

    /// <summary>Manifest JSON from packaging-agent (artifact + file references, no archive).</summary>
    public const string PackagingManifest = "packaging-manifest";
}
