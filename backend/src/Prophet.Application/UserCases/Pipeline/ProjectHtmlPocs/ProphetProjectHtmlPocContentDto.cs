namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

/// <summary>UTF-8 HTML body for in-app preview (served from API to avoid browser CORS on GCS signed URLs).</summary>
public sealed record PipelineHtmlPocContentDto(string Text);
