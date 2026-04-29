using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Services.EntityId;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

/// <summary>
/// After a refined version is created, duplicates parent inputs and preserved step outputs into the new version (same storage paths for blobs).
/// </summary>
public sealed class CopyPipelineOutputsFromParentVersionUseCase(
    IPipelineExecutionStore pipelineStore,
    IEntityIdGenerator idGenerator)
{
    /// <param name="startFromStepInclusive">First pipeline step that will run on the new version; steps below this are copied from the parent.</param>
    public async Task ExecuteAsync(
        Guid parentVersionId,
        Guid newVersionId,
        int startFromStepInclusive,
        CancellationToken cancellationToken = default)
    {
        var (artifactTypes, fileTypes) = PipelineStepCatalog.CollectOutputsBeforeStepExclusive(startFromStepInclusive);

        var parentFiles = await pipelineStore.ListFilesAsync(parentVersionId, cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;

        foreach (var f in parentFiles.Where(x => x.FileType == ArtifactFileTypeNames.Input))
        {
            await pipelineStore
                .AddFileAsync(
                    new PipelineVersionFile
                    {
                        Id = idGenerator.NewId(),
                        VersionId = newVersionId,
                        FileType = f.FileType,
                        StorageObjectPath = f.StorageObjectPath,
                        OriginalFileName = f.OriginalFileName,
                        CreatedAtUtc = now,
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }

        foreach (var artifactType in artifactTypes)
        {
            var src = await pipelineStore
                .GetArtifactByTypeAsync(parentVersionId, artifactType, cancellationToken)
                .ConfigureAwait(false);
            if (src == null)
                continue;

            await pipelineStore
                .AddArtifactAsync(
                    new PipelineArtifact
                    {
                        Id = idGenerator.NewId(),
                        VersionId = newVersionId,
                        ArtifactType = src.ArtifactType,
                        ContentJson = src.ContentJson,
                        CreatedByAgent = src.CreatedByAgent,
                        CreatedAtUtc = now,
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }

        foreach (var fileType in fileTypes)
        {
            foreach (var f in parentFiles.Where(x => x.FileType == fileType))
            {
                await pipelineStore
                    .AddFileAsync(
                        new PipelineVersionFile
                        {
                            Id = idGenerator.NewId(),
                            VersionId = newVersionId,
                            FileType = f.FileType,
                            StorageObjectPath = f.StorageObjectPath,
                            OriginalFileName = f.OriginalFileName,
                            CreatedAtUtc = now,
                        },
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
