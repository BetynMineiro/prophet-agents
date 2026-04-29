using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.Projects.Validators;

public class UpdatePipelineProjectRequestValidator : IValidator<UpdatePipelineProjectRequest>
{
    public ValidationResult Validate(UpdatePipelineProjectRequest? value)
    {
        var errors = new List<string>();
        if (value == null)
        {
            errors.Add("Request is required.");
            return ValidationResult.Fail(errors);
        }

        if (string.IsNullOrWhiteSpace(value.Name))
            errors.Add("Name is required.");
        else if (value.Name.Trim().Length > 256)
            errors.Add("Name must be at most 256 characters.");

        if (!string.IsNullOrWhiteSpace(value.Description) && value.Description.Trim().Length > 4096)
            errors.Add("Description must be at most 4096 characters.");

        return errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.Fail(errors);
    }
}
