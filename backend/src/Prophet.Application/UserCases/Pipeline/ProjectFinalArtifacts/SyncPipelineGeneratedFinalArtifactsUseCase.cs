using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.Options;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public sealed class SyncPipelineGeneratedFinalArtifactsUseCase(
    IPipelineProjectStore projectStore,
    IPipelineExecutionStore pipelineStore,
    IPipelineFinalArtifactStore finalArtifactStore,
    IStorageService storage,
    IOptions<StorageOptions> storageOptions,
    ILogger<SyncPipelineGeneratedFinalArtifactsUseCase> logger) : ISyncPipelineGeneratedFinalArtifactsUseCase
{
    private const string OwnerSegment = "prophet";
    private const string AssetType = "final-artifacts";

    public async Task ExecuteAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default)
    {
        var project = await projectStore.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project == null || project.DeletedAtUtc != null)
            return;

        var version = await pipelineStore.GetVersionForProjectAsync(projectId, versionId, cancellationToken).ConfigureAwait(false);
        if (version == null)
            return;

        var versionLabel = $"version {version.VersionNumber}";

        await RemovePreviousPipelineSyncArtifactsAsync(projectId, cancellationToken).ConfigureAwait(false);

        var root = storageOptions.Value.Root;
        if (string.IsNullOrWhiteSpace(root))
            root = "genesis";
        var productSegment = projectId.ToString("D");

        var arch = await pipelineStore.GetArtifactByTypeAsync(versionId, ArtifactTypeNames.Architecture, cancellationToken).ConfigureAwait(false);
        if (arch != null && !string.IsNullOrWhiteSpace(arch.ContentJson))
        {
            var md = PipelineFinalArtifactSyncFormatter.BuildArchitectureMarkdown(versionLabel, arch.ContentJson);
            await UploadMarkdownAsync(projectId, root, productSegment, PipelineGeneratedFinalArtifactNames.Architecture, md, cancellationToken).ConfigureAwait(false);
        }

        var classDg = await pipelineStore.GetArtifactByTypeAsync(versionId, ArtifactTypeNames.ClassDiagram, cancellationToken).ConfigureAwait(false);
        if (classDg != null && !string.IsNullOrWhiteSpace(classDg.ContentJson))
        {
            var md = PipelineFinalArtifactSyncFormatter.BuildDiagramMarkdown("Class diagram", versionLabel, classDg.ContentJson);
            await UploadMarkdownAsync(projectId, root, productSegment, PipelineGeneratedFinalArtifactNames.ClassDiagram, md, cancellationToken).ConfigureAwait(false);
        }

        var flowDg = await pipelineStore.GetArtifactByTypeAsync(versionId, ArtifactTypeNames.FlowDiagram, cancellationToken).ConfigureAwait(false);
        if (flowDg != null && !string.IsNullOrWhiteSpace(flowDg.ContentJson))
        {
            var md = PipelineFinalArtifactSyncFormatter.BuildDiagramMarkdown("Flow diagram", versionLabel, flowDg.ContentJson);
            await UploadMarkdownAsync(projectId, root, productSegment, PipelineGeneratedFinalArtifactNames.FlowDiagram, md, cancellationToken).ConfigureAwait(false);
        }

        var files = await pipelineStore.ListFilesAsync(versionId, cancellationToken).ConfigureAwait(false);
        var docFile = files
            .Where(f => f.FileType == ArtifactFileTypeNames.Documentation)
            .OrderBy(f => f.CreatedAtUtc)
            .FirstOrDefault();
        if (docFile != null)
        {
            var bytes = await storage.ReadObjectAsync(docFile.StorageObjectPath, cancellationToken).ConfigureAwait(false);
            if (bytes is { Length: > 0 })
            {
                var text = Encoding.UTF8.GetString(bytes);
                await UploadMarkdownAsync(projectId, root, productSegment, PipelineGeneratedFinalArtifactNames.Documentation, text, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task RemovePreviousPipelineSyncArtifactsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var existing = await finalArtifactStore.ListByProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        var nameSet = new HashSet<string>(PipelineGeneratedFinalArtifactNames.All, StringComparer.OrdinalIgnoreCase);
        foreach (var x in existing.Where(e => nameSet.Contains(e.OriginalFileName)))
        {
            try
            {
                await storage.DeleteObjectAsync(x.StorageObjectPath, cancellationToken).ConfigureAwait(false);
                await finalArtifactStore.DeleteAsync(projectId, x.Id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to remove previous pipeline-sync final artifact {Name} for project {ProjectId}",
                    x.OriginalFileName,
                    projectId);
            }
        }
    }

    private async Task UploadMarkdownAsync(
        Guid projectId,
        string root,
        string productSegment,
        string displayName,
        string markdown,
        CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(markdown);
        if (bytes.Length > FinalArtifactUploadLimits.MaxFileBytes)
        {
            logger.LogWarning(
                "Skipping pipeline final artifact {Name} for project {ProjectId}: size {Size} exceeds {Limit} bytes",
                displayName,
                projectId,
                bytes.Length,
                FinalArtifactUploadLimits.MaxFileBytes);
            return;
        }

        var docId = Guid.NewGuid();
        var storageFileName = $"{docId:N}_{displayName}";
        await using var stream = new MemoryStream(bytes, writable: false);
        var objectPath = await storage.UploadAsync(
            root,
            OwnerSegment,
            productSegment,
            AssetType,
            storageFileName,
            stream,
            "text/markdown",
            cancellationToken).ConfigureAwait(false);

        var entity = new PipelineFinalArtifact
        {
            Id = docId,
            PipelineProjectId = projectId,
            OriginalFileName = displayName,
            ContentType = "text/markdown",
            StorageObjectPath = objectPath,
            SizeBytes = bytes.Length,
        };
        entity.SetCreatedBy(Guid.Empty);
        await finalArtifactStore.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }
}
