using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Domain.Pipeline;

/// <summary>Guards catalog length for the MAF pipeline (must match ordered agents in Application).</summary>
public sealed class MainPipelineStepIdsTests
{
    [Fact]
    public void TotalSteps_matches_StepIds_count()
    {
        Assert.Equal(MainPipelineStepIds.StepIds.Count, MainPipelineStepIds.TotalSteps);
    }

    [Fact]
    public void StepIds_has_expected_length_for_maf_pipeline()
    {
        Assert.Equal(10, MainPipelineStepIds.StepIds.Count);
    }
}
