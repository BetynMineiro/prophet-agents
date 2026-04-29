using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.Validators;

public sealed class PipelineProjectIdQueryValidator : IValidator<PipelineProjectIdQuery>
{
    public ValidationResult Validate(PipelineProjectIdQuery value)
    {
        if (value.ProjectId == Guid.Empty)
            return ValidationResult.Fail(["Project id is required."]);
        return ValidationResult.Ok();
    }
}
