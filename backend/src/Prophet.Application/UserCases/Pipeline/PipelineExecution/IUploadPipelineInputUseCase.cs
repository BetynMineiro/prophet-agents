namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public interface IUploadPipelineInputUseCase
{
    /// <param name="versionId">When null, a new artifact version row is created for this project.</param>
    Task<PipelineVersionFileItemDto?> ExecuteAsync(
        Guid projectId,
        Guid? versionId,
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
