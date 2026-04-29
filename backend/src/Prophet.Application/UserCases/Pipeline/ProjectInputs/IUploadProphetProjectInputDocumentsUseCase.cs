namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

public interface IUploadPipelineInputDocumentsUseCase
{
    Task<UploadPipelineInputDocumentsResponseDto?> ExecuteAsync(
        Guid projectId,
        IReadOnlyList<InputFileChunk> files,
        CancellationToken cancellationToken = default);
}
