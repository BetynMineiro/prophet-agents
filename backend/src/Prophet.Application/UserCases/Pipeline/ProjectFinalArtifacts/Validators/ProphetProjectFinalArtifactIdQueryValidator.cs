using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts.Validators;

public sealed class PipelineFinalArtifactIdQueryValidator : IValidator<PipelineFinalArtifactIdQuery>
{
    public ValidationResult Validate(PipelineFinalArtifactIdQuery value)
    {
        var errors = new List<string>();
        if (value.ProjectId == Guid.Empty)
            errors.Add("Project id is required.");
        if (value.DocumentId == Guid.Empty)
            errors.Add("Document id is required.");
        return errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.Fail(errors);
    }
}
