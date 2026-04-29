namespace Prophet.Domain.Entities.Pipeline;

/// <summary>Maps pipeline step index to persisted outputs (for rewind / retry cleanup).</summary>
public static class PipelineStepCatalog
{
    /// <summary>JSONB artifact types produced when step <paramref name="stepIndex"/> completes.</summary>
    public static IReadOnlyList<string> ArtifactTypesProducedAtStep(int stepIndex) => stepIndex switch
    {
        0 => [ArtifactTypeNames.Chunks],
        1 => [ArtifactTypeNames.Insights],
        2 => [ArtifactTypeNames.MarketAnalysis],
        3 => [ArtifactTypeNames.DomainModel],
        4 => [ArtifactTypeNames.Architecture],
        5 => [ArtifactTypeNames.ClassDiagram, ArtifactTypeNames.FlowDiagram],
        6 => [],
        7 => [],
        8 => [],
        9 => [ArtifactTypeNames.PackagingManifest],
        _ => [],
    };

    /// <summary>Version file types produced when step <paramref name="stepIndex"/> completes (storage rows).</summary>
    public static IReadOnlyList<string> FileTypesProducedAtStep(int stepIndex) => stepIndex switch
    {
        6 => [ArtifactFileTypeNames.PocWeb],
        7 => [ArtifactFileTypeNames.PocMobile],
        8 => [ArtifactFileTypeNames.Documentation],
        _ => [],
    };

    /// <summary>All artifact types from <paramref name="fromStepInclusive"/> through last step.</summary>
    public static HashSet<string> CollectArtifactTypesFromStepInclusive(int fromStepInclusive)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        var max = MainPipelineStepIds.TotalSteps - 1;
        for (var s = fromStepInclusive; s <= max; s++)
        {
            foreach (var t in ArtifactTypesProducedAtStep(s))
                set.Add(t);
        }

        return set;
    }

    /// <summary>
    /// Artifact and file types produced by steps strictly before <paramref name="startFromStepInclusive"/>
    /// (the first step that will be re-executed after a refinement).
    /// </summary>
    public static (HashSet<string> ArtifactTypes, HashSet<string> FileTypes) CollectOutputsBeforeStepExclusive(int startFromStepInclusive)
    {
        var artifacts = new HashSet<string>(StringComparer.Ordinal);
        var files = new HashSet<string>(StringComparer.Ordinal);
        if (startFromStepInclusive <= 0)
            return (artifacts, files);

        var lastStepToCopy = startFromStepInclusive - 1;
        for (var s = 0; s <= lastStepToCopy && s < MainPipelineStepIds.TotalSteps; s++)
        {
            foreach (var t in ArtifactTypesProducedAtStep(s))
                artifacts.Add(t);
            foreach (var t in FileTypesProducedAtStep(s))
                files.Add(t);
        }

        return (artifacts, files);
    }

    /// <summary>All file types from <paramref name="fromStepInclusive"/> through last step.</summary>
    public static HashSet<string> CollectFileTypesFromStepInclusive(int fromStepInclusive)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        var max = MainPipelineStepIds.TotalSteps - 1;
        for (var s = fromStepInclusive; s <= max; s++)
        {
            foreach (var t in FileTypesProducedAtStep(s))
                set.Add(t);
        }

        return set;
    }
}
