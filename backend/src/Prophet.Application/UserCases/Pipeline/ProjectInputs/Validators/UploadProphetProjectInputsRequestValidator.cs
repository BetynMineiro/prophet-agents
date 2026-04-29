using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.ProjectInputs.Validators;

public sealed class UploadPipelineInputDocumentsRequestValidator : IValidator<UploadPipelineInputDocumentsRequest>
{
    public const int MaxFilesPerRequest = 40;

    public ValidationResult Validate(UploadPipelineInputDocumentsRequest? value)
    {
        var errors = new List<string>();
        if (value == null)
        {
            errors.Add("Request is required.");
            return ValidationResult.Fail(errors);
        }

        if (value.ProjectId == Guid.Empty)
            errors.Add("Project id is required.");

        if (value.Files == null)
            errors.Add("Files are required.");
        else
        {
            if (value.Files.Count == 0)
                errors.Add("At least one file is required.");
            else if (value.Files.Count > MaxFilesPerRequest)
                errors.Add($"At most {MaxFilesPerRequest} files per request.");

            foreach (var chunk in value.Files)
            {
                if (string.IsNullOrEmpty(chunk.SkipReason) && chunk.Content.Length > UploadPipelineInputDocumentsUseCase.MaxFileBytes)
                    errors.Add($"File exceeds {UploadPipelineInputDocumentsUseCase.MaxFileBytes / (1024 * 1024)} MB limit.");
                else if (string.IsNullOrEmpty(chunk.SkipReason) && !PipelineTextInputFileRules.IsAllowedExtension(chunk.OriginalFileName))
                    errors.Add(
                        $"\"{chunk.OriginalFileName}\": extension not allowed. Allowed types include {PipelineTextInputFileRules.AllowedHint}.");
            }
        }

        return errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.Fail(errors.Distinct().ToList());
    }
}
