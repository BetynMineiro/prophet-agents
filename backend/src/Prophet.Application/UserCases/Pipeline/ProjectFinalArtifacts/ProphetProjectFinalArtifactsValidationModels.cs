using Prophet.Application.UserCases.Pipeline.ProjectInputs;

namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

/// <summary>Multipart upload: project id + file chunks (only .md allowed in use case).</summary>
public sealed record UploadPipelineFinalArtifactsRequest(
    Guid ProjectId,
    IReadOnlyList<InputFileChunk> Files);

/// <summary>Identifies a single final artifact under a project.</summary>
public readonly record struct PipelineFinalArtifactIdQuery(Guid ProjectId, Guid DocumentId);
