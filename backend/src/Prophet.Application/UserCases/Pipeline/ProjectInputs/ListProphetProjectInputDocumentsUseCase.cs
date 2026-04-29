using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

public sealed class ListPipelineInputDocumentsUseCase(
    IPipelineProjectStore projectStore,
    IPipelineInputDocumentStore documentStore,
    IValidator<PipelineProjectIdQuery> validator,
    IValidationErrorCollector errorCollector) : IListPipelineInputDocumentsUseCase
{
    public async Task<IReadOnlyList<PipelineInputDocumentItemDto>?> ExecuteAsync(
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

        var list = await documentStore.ListByProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        return list.Select(static x => new PipelineInputDocumentItemDto(
            x.Id,
            x.OriginalFileName,
            x.ContentType,
            x.SizeBytes,
            x.CreatedAtUtc)).ToList();
    }
}
