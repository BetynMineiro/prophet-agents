using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

/// <summary>Deletes JSON artifacts and version files from <paramref name="fromStepInclusive"/> onward (storage + DB). Preserves input uploads.</summary>
public sealed class ClearPipelineOutputsFromStepUseCase(IPipelineExecutionStore pipelineStore, IStorageService storage)
{
    public async Task ExecuteAsync(Guid versionId, int fromStepInclusive, CancellationToken cancellationToken = default)
    {
        var max = MainPipelineStepIds.TotalSteps - 1;
        if (fromStepInclusive < 0 || fromStepInclusive > max)
            return;

        var artifactTypes = PipelineStepCatalog.CollectArtifactTypesFromStepInclusive(fromStepInclusive);
        var fileTypes = PipelineStepCatalog.CollectFileTypesFromStepInclusive(fromStepInclusive);

        var files = await pipelineStore.ListFilesAsync(versionId, cancellationToken).ConfigureAwait(false);
        var fileTypeSet = fileTypes.ToHashSet(StringComparer.Ordinal);
        var toRemove = files.Where(f => fileTypeSet.Contains(f.FileType)).ToList();
        foreach (var f in toRemove)
        {
            await storage.DeleteObjectAsync(f.StorageObjectPath, cancellationToken).ConfigureAwait(false);
        }

        if (toRemove.Count > 0)
        {
            await pipelineStore.DeleteFilesByIdsAsync(
                versionId,
                toRemove.Select(f => f.Id).ToList(),
                cancellationToken).ConfigureAwait(false);
        }

        if (artifactTypes.Count > 0)
        {
            await pipelineStore.DeleteArtifactsAsync(versionId, artifactTypes.ToList(), cancellationToken).ConfigureAwait(false);
        }
    }
}
