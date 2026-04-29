namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

/// <summary>UTF-8 Markdown body for in-app preview (served from API to avoid browser CORS on GCS signed URLs).</summary>
public sealed record PipelineFinalArtifactContentDto(string Text);
