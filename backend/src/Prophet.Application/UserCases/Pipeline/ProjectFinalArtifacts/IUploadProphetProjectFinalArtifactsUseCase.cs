using Prophet.Application.UserCases.Pipeline.ProjectInputs;

namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public interface IUploadPipelineFinalArtifactsUseCase
{
    Task<UploadPipelineFinalArtifactsResponseDto?> ExecuteAsync(
        Guid projectId,
        IReadOnlyList<InputFileChunk> files,
        CancellationToken cancellationToken = default);
}
