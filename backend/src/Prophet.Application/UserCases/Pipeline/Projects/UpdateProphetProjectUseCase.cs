using Prophet.Application.Interfaces.Pipeline;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.Projects;

public class UpdatePipelineProjectUseCase(
    IPipelineProjectStore store,
    IValidator<UpdatePipelineProjectRequest> validator,
    IValidationErrorCollector errorCollector) : IUpdatePipelineProjectUseCase
{
    public async Task<PipelineProjectItemDto?> ExecuteAsync(Guid id, UpdatePipelineProjectRequest request, CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                errorCollector.AddError(error);
            return null;
        }

        var userId = Guid.Empty;
        var name = request.Name.Trim();
        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        var updated = await store.UpdateAsync(id, name, description, request.ExpectedDate, request.IsActive, userId, cancellationToken).ConfigureAwait(false);
        if (updated == null)
            return null;

        var latestByProject = await store.GetLatestArtifactVersionPipelineByProjectIdsAsync([id], cancellationToken).ConfigureAwait(false);
        return PipelineProjectItemDto.FromProject(
            updated,
            latestByProject.TryGetValue(id, out var tup) ? tup : null);
    }
}
