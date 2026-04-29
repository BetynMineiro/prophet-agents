using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Services.EntityId;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.Projects;

public class CreatePipelineProjectUseCase(
    IPipelineProjectStore store,
    IEntityIdGenerator idGenerator,
    IValidator<CreatePipelineProjectRequest> validator,
    IValidationErrorCollector errorCollector) : ICreatePipelineProjectUseCase
{
    public async Task<PipelineProjectItemDto?> ExecuteAsync(CreatePipelineProjectRequest request, CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                errorCollector.AddError(error);
            return null;
        }

        var userId = Guid.Empty;
        var entity = new PipelineProject
        {
            Id = idGenerator.NewId(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ExpectedDate = request.ExpectedDate,
            DeletedAtUtc = (request.IsActive ?? true) ? null : DateTime.UtcNow
        };
        entity.SetCreatedBy(userId);

        var created = await store.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        return PipelineProjectItemDto.FromProject(created, null);
    }
}
