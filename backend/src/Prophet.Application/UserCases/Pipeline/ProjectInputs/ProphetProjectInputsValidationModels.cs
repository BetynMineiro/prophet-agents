namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

/// <summary>Multipart upload: project id + file chunks.</summary>
public sealed record UploadPipelineInputDocumentsRequest(
    Guid ProjectId,
    IReadOnlyList<InputFileChunk> Files);

/// <summary>Identifies a single input document under a project.</summary>
public readonly record struct PipelineInputDocumentIdQuery(Guid ProjectId, Guid DocumentId);
