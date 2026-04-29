using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Services.EntityId;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class RefinePipelineProjectUseCase(
    IPipelineExecutionStore pipelineStore,
    IEntityIdGenerator idGenerator,
    IRefinementStartStepResolver refinementStartStepResolver,
    CopyPipelineOutputsFromParentVersionUseCase copyPipelineOutputsFromParentVersion) : IRefinePipelineProjectUseCase
{
    public async Task<RefinePipelineProjectResponseDto?> ExecuteAsync(
        Guid projectId,
        RefinePipelineProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ChangeSummary))
            return null;

        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return null;

        var max = await pipelineStore.GetMaxVersionNumberAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (max < 1)
            return null;

        var latest = await pipelineStore.ListVersionsPageAsync(projectId, 1, null, cancellationToken).ConfigureAwait(false);
        var parent = latest.Items.Count == 0 ? null : latest.Items[0];

        var startFrom = await refinementStartStepResolver
            .ResolveStartStepIndexAsync(request.ChangeSummary.Trim(), cancellationToken)
            .ConfigureAwait(false);

        var userId = Guid.Empty;
        var next = max + 1;
        var entity = new ArtifactVersion
        {
            Id = idGenerator.NewId(),
            PipelineProjectId = projectId,
            VersionNumber = next,
            ParentVersionId = parent?.Id,
            ChangeSummary = request.ChangeSummary.Trim(),
            PipelineStatus = PipelineRunStatus.Idle,
            CurrentStepIndex = startFrom,
        };
        entity.SetCreatedBy(userId);
        var created = await pipelineStore.AddVersionAsync(entity, cancellationToken).ConfigureAwait(false);

        if (parent != null)
        {
            await copyPipelineOutputsFromParentVersion
                .ExecuteAsync(parent.Id, created.Id, startFrom, cancellationToken)
                .ConfigureAwait(false);
        }

        return new RefinePipelineProjectResponseDto(created.Id, created.VersionNumber, startFrom);
    }
}
