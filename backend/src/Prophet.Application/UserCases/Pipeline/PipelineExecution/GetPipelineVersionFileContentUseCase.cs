using System.Text;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class GetPipelineVersionFileContentUseCase(
    IPipelineExecutionStore pipelineStore,
    IStorageService storage) : IGetPipelineVersionFileContentUseCase
{
    public async Task<PipelineVersionFileContentDto?> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return null;

        if (await pipelineStore.GetVersionForProjectAsync(projectId, versionId, cancellationToken).ConfigureAwait(false) == null)
            return null;

        var file = await pipelineStore.GetFileAsync(versionId, fileId, cancellationToken).ConfigureAwait(false);
        if (file == null)
            return null;

        var bytes = await storage.ReadObjectAsync(file.StorageObjectPath, cancellationToken).ConfigureAwait(false);
        if (bytes == null || bytes.Length == 0)
            return null;

        if (bytes.Length > PipelineVersionUploadLimits.MaxFileBytes)
            return null;

        var text = Encoding.UTF8.GetString(bytes);
        return new PipelineVersionFileContentDto(text);
    }
}
