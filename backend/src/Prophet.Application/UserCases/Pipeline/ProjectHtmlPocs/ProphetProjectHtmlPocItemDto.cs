using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public sealed record PipelineHtmlPocItemDto(
    Guid Id,
    HtmlPocKind Kind,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAtUtc);
