namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

/// <summary>
/// Reserved <see cref="PipelineFinalArtifact.OriginalFileName"/> values produced by
/// <see cref="ISyncPipelineGeneratedFinalArtifactsUseCase"/> so each pipeline completion replaces the same set.
/// Manual uploads should use other names to avoid confusion.
/// </summary>
public static class PipelineGeneratedFinalArtifactNames
{
    public const string Architecture = "pipeline-architecture.md";
    public const string ClassDiagram = "pipeline-class-diagram.md";
    public const string FlowDiagram = "pipeline-flow-diagram.md";
    public const string Documentation = "pipeline-documentation.md";

    public static readonly IReadOnlyList<string> All =
    [
        Architecture,
        ClassDiagram,
        FlowDiagram,
        Documentation,
    ];
}
