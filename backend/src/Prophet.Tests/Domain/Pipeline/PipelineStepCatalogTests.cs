using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Tests.Domain.Pipeline;

public sealed class PipelineStepCatalogTests
{
    [Fact]
    public void CollectOutputsBeforeStepExclusive_0_is_empty()
    {
        var (artifacts, files) = PipelineStepCatalog.CollectOutputsBeforeStepExclusive(0);
        Assert.Empty(artifacts);
        Assert.Empty(files);
    }

    [Fact]
    public void CollectOutputsBeforeStepExclusive_4_includes_model_not_architecture()
    {
        var (artifacts, files) = PipelineStepCatalog.CollectOutputsBeforeStepExclusive(4);
        Assert.Contains(ArtifactTypeNames.DomainModel, artifacts);
        Assert.DoesNotContain(ArtifactTypeNames.Architecture, artifacts);
        Assert.Empty(files);
    }

    [Fact]
    public void CollectOutputsBeforeStepExclusive_9_includes_poc_files()
    {
        var (_, files) = PipelineStepCatalog.CollectOutputsBeforeStepExclusive(9);
        Assert.Contains(ArtifactFileTypeNames.PocWeb, files);
        Assert.Contains(ArtifactFileTypeNames.PocMobile, files);
        Assert.Contains(ArtifactFileTypeNames.Documentation, files);
    }
}
