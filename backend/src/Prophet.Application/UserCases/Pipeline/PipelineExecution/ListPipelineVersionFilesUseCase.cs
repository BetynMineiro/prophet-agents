using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class ListPipelineVersionFilesUseCase(
    IPipelineExecutionStore pipelineStore,
    IStorageService storage) : IListPipelineVersionFilesUseCase
{
    private static readonly TimeSpan SignedUrlDuration = TimeSpan.FromHours(1);

    public async Task<IReadOnlyList<PipelineVersionFileItemDto>?> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return null;

        if (await pipelineStore.GetVersionForProjectAsync(projectId, versionId, cancellationToken).ConfigureAwait(false) == null)
            return null;

        var files = await pipelineStore.ListFilesAsync(versionId, cancellationToken).ConfigureAwait(false);
        var list = new List<PipelineVersionFileItemDto>(files.Count);
        foreach (var f in files)
        {
            // Inline signed URLs (no attachment disposition) so HTML can load in an iframe preview.
            // Clients that need a forced download can pass filename when generating a separate URL.
            var url = await storage.GetSignedUrlAsync(
                f.StorageObjectPath,
                SignedUrlDuration,
                downloadAsFileName: null,
                cancellationToken).ConfigureAwait(false);
            list.Add(new PipelineVersionFileItemDto(
                f.Id,
                f.FileType,
                f.StorageObjectPath,
                f.OriginalFileName,
                url,
                f.CreatedAtUtc));
        }

        return list;
    }
}
