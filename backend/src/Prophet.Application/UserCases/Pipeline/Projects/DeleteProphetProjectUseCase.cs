using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.Projects;

public class DeletePipelineProjectUseCase(
    IPipelineProjectStore store,
    IValidator<PipelineProjectIdQuery> validator,
    IValidationErrorCollector errorCollector) : IDeletePipelineProjectUseCase
{
    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(new PipelineProjectIdQuery(id));
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                errorCollector.AddError(error);
            return false;
        }

        var userId = Guid.Empty;
        return await store.SoftDeleteAsync(id, userId, cancellationToken).ConfigureAwait(false);
    }
}
