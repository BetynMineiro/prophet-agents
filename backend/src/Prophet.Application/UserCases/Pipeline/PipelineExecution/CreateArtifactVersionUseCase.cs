using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Services.EntityId;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class CreateArtifactVersionUseCase(
    IPipelineExecutionStore pipelineStore,
    IEntityIdGenerator idGenerator) : ICreateArtifactVersionUseCase
{
    public async Task<ArtifactVersionItemDto?> ExecuteAsync(
        Guid projectId,
        CreateArtifactVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return null;

        if (request.ParentVersionId is { } parentId)
        {
            var parent = await pipelineStore.GetVersionForProjectAsync(projectId, parentId, cancellationToken).ConfigureAwait(false);
            if (parent == null)
                return null;
        }

        var userId = Guid.Empty;
        var next = await pipelineStore.GetMaxVersionNumberAsync(projectId, cancellationToken).ConfigureAwait(false) + 1;
        var entity = new ArtifactVersion
        {
            Id = idGenerator.NewId(),
            PipelineProjectId = projectId,
            VersionNumber = next,
            ParentVersionId = request.ParentVersionId,
            ChangeSummary = string.IsNullOrWhiteSpace(request.ChangeSummary) ? null : request.ChangeSummary.Trim(),
            PipelineStatus = PipelineRunStatus.Idle,
            CurrentStepIndex = 0,
        };
        entity.SetCreatedBy(userId);
        var created = await pipelineStore.AddVersionAsync(entity, cancellationToken).ConfigureAwait(false);
        return PipelineExecutionStatusHelper.ToItemDto(created);
    }
}
