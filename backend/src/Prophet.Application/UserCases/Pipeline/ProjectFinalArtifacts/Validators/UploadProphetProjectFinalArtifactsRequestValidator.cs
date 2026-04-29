using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts.Validators;

public sealed class UploadPipelineFinalArtifactsRequestValidator : IValidator<UploadPipelineFinalArtifactsRequest>
{
    public ValidationResult Validate(UploadPipelineFinalArtifactsRequest? value)
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
            else if (value.Files.Count > FinalArtifactUploadLimits.MaxFilesPerRequest)
                errors.Add($"At most {FinalArtifactUploadLimits.MaxFilesPerRequest} files per request.");

            foreach (var chunk in value.Files)
            {
                if (string.IsNullOrEmpty(chunk.SkipReason) && chunk.Content.Length > FinalArtifactUploadLimits.MaxFileBytes)
                    errors.Add($"File exceeds {FinalArtifactUploadLimits.MaxFileBytes / (1024 * 1024)} MB limit.");
            }
        }

        return errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.Fail(errors.Distinct().ToList());
    }
}
