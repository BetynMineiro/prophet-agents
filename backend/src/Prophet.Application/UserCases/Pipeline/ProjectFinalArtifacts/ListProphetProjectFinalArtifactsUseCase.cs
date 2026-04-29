using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public sealed class ListPipelineFinalArtifactsUseCase(
    IPipelineProjectStore projectStore,
    IPipelineFinalArtifactStore artifactStore,
    IValidator<PipelineProjectIdQuery> validator,
    IValidationErrorCollector errorCollector) : IListPipelineFinalArtifactsUseCase
{
    public async Task<IReadOnlyList<PipelineFinalArtifactItemDto>?> ExecuteAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(new PipelineProjectIdQuery(projectId));
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                errorCollector.AddError(error);
            return null;
        }

        var project = await projectStore.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project == null)
            return null;

        var list = await artifactStore.ListByProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        return list.Select(static x => new PipelineFinalArtifactItemDto(
            x.Id,
            x.OriginalFileName,
            x.ContentType,
            x.SizeBytes,
            x.CreatedAtUtc)).ToList();
    }
}
