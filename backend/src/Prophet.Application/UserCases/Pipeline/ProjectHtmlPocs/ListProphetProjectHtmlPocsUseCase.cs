using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public sealed class ListPipelineHtmlPocsUseCase(
    IPipelineProjectStore projectStore,
    IPipelineHtmlPocStore pocStore,
    IValidator<PipelineProjectIdQuery> validator,
    IValidationErrorCollector errorCollector) : IListPipelineHtmlPocsUseCase
{
    public async Task<IReadOnlyList<PipelineHtmlPocItemDto>?> ExecuteAsync(
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

        var list = await pocStore.ListByProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        return list.Select(static x => new PipelineHtmlPocItemDto(
            x.Id,
            x.PocKind,
            x.OriginalFileName,
            x.ContentType,
            x.SizeBytes,
            x.CreatedAtUtc)).ToList();
    }
}
