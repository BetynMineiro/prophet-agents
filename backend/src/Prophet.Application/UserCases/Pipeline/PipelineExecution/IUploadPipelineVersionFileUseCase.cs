namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

/// <summary>
/// Uploads a blob under <c>ai-artifacts/{projectId}/v{n}/{folder}/</c> and persists <see cref="Prophet.Domain.Entities.Pipeline.PipelineVersionFile"/>.
/// Used by <see cref="IUploadPipelineInputUseCase"/> and pipeline agents.
/// </summary>
public interface IUploadPipelineVersionFileUseCase
{
    /// <param name="storageFolderSegment">Segment under <c>v{n}/</c>, e.g. <c>input</c>, <c>poc-web</c>, <c>documentation</c>.</param>
    Task<PipelineVersionFileItemDto?> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        int versionNumber,
        string storageFolderSegment,
        string fileType,
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
