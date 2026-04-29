using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectInputs.Validators;

public sealed class PipelineInputDocumentIdQueryValidator : IValidator<PipelineInputDocumentIdQuery>
{
    public ValidationResult Validate(PipelineInputDocumentIdQuery value)
    {
        var errors = new List<string>();
        if (value.ProjectId == Guid.Empty)
            errors.Add("Project id is required.");
        if (value.DocumentId == Guid.Empty)
            errors.Add("Document id is required.");
        return errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.Fail(errors);
    }
}
