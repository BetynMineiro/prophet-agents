using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Services.EntityId;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class AddPipelineArtifactUseCase(IPipelineExecutionStore pipelineStore, IEntityIdGenerator idGenerator)
    : IAddPipelineArtifactUseCase
{
    public async Task ExecuteAsync(
        Guid versionId,
        string artifactType,
        string contentJson,
        string createdByAgent,
        CancellationToken cancellationToken = default)
    {
        var entity = new PipelineArtifact
        {
            Id = idGenerator.NewId(),
            VersionId = versionId,
            ArtifactType = artifactType,
            ContentJson = contentJson,
            CreatedByAgent = createdByAgent,
            CreatedAtUtc = DateTime.UtcNow,
        };
        await pipelineStore.AddArtifactAsync(entity, cancellationToken).ConfigureAwait(false);
    }
}
