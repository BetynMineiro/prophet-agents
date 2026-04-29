namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

/// <summary>Limits for final Markdown artifact uploads (per file and per request).</summary>
public static class FinalArtifactUploadLimits
{
    public const long MaxFileBytes = 5L * 1024 * 1024;
    public const int MaxFilesPerRequest = 40;
}
