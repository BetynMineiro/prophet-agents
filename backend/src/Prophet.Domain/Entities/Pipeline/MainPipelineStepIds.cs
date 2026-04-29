namespace Prophet.Domain.Entities.Pipeline;

/// <summary>Ordered architecture-generation pipeline steps (§3.1).</summary>
public static class MainPipelineStepIds
{
    public static readonly IReadOnlyList<string> StepIds =
    [
        "file",
        "insight",
        "market",
        "model",
        "architecture",
        "diagram",
        "poc-web",
        "poc-mobile",
        "doc",
        "packaging"
    ];

    public static int TotalSteps => StepIds.Count;
}
