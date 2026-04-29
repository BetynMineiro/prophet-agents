using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.Projects;

/// <summary>Clears soft-delete on a Prophet project so it becomes active again.</summary>
public class RestorePipelineProjectUseCase(
    IPipelineProjectStore store,
    IValidator<PipelineProjectIdQuery> validator,
    IValidationErrorCollector errorCollector) : IRestorePipelineProjectUseCase
{
    public async Task<PipelineProjectItemDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(new PipelineProjectIdQuery(id));
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                errorCollector.AddError(error);
            return null;
        }

        var userId = Guid.Empty;
        var restored = await store.RestoreAsync(id, userId, cancellationToken).ConfigureAwait(false);
        if (restored == null)
            return null;
        var latestByProject = await store.GetLatestArtifactVersionPipelineByProjectIdsAsync([id], cancellationToken).ConfigureAwait(false);
        return PipelineProjectItemDto.FromProject(
            restored,
            latestByProject.TryGetValue(id, out var tup) ? tup : null);
    }
}
