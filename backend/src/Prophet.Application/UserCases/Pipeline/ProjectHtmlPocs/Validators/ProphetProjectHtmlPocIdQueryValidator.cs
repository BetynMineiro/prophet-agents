using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs.Validators;

public sealed class PipelineHtmlPocIdQueryValidator : IValidator<PipelineHtmlPocIdQuery>
{
    public ValidationResult Validate(PipelineHtmlPocIdQuery value)
    {
        var errors = new List<string>();
        if (value.ProjectId == Guid.Empty)
            errors.Add("Project id is required.");
        if (value.DocumentId == Guid.Empty)
            errors.Add("Document id is required.");
        return errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.Fail(errors);
    }
}
