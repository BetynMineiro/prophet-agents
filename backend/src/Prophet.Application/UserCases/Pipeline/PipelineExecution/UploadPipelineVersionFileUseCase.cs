using Microsoft.Extensions.Options;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.Options;
using Prophet.Application.Services.EntityId;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class UploadPipelineVersionFileUseCase(
    IPipelineExecutionStore pipelineStore,
    IStorageService storage,
    IOptions<StorageOptions> storageOptions,
    IEntityIdGenerator idGenerator) : IUploadPipelineVersionFileUseCase
{
    public const long DefaultMaxFileBytes = PipelineVersionUploadLimits.MaxFileBytes;

    public async Task<PipelineVersionFileItemDto?> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        int versionNumber,
        string storageFolderSegment,
        string fileType,
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return null;

        var version = await pipelineStore.GetVersionForProjectAsync(projectId, versionId, cancellationToken).ConfigureAwait(false);
        if (version == null || version.VersionNumber != versionNumber)
            return null;

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (buffer.Length > DefaultMaxFileBytes)
            return null;

        var folder = NormalizeFolderSegment(storageFolderSegment);
        if (folder == null)
            return null;

        var root = storageOptions.Value.Root;
        if (string.IsNullOrWhiteSpace(root))
            root = "genesis";

        var safeName = SanitizeFileName(originalFileName);
        var storageFileName = $"{Guid.NewGuid():N}_{safeName}";
        var ct = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim();

        buffer.Position = 0;
        var objectPath = await storage.UploadAsync(
            root,
            "ai-artifacts",
            projectId.ToString("D"),
            $"v{versionNumber}/{folder}",
            storageFileName,
            buffer,
            ct,
            cancellationToken).ConfigureAwait(false);

        var fileEntity = new PipelineVersionFile
        {
            Id = idGenerator.NewId(),
            VersionId = versionId,
            FileType = fileType,
            StorageObjectPath = objectPath,
            OriginalFileName = safeName,
            CreatedAtUtc = DateTime.UtcNow,
        };
        var created = await pipelineStore.AddFileAsync(fileEntity, cancellationToken).ConfigureAwait(false);
        var url = await storage.GetSignedUrlAsync(
            created.StorageObjectPath,
            TimeSpan.FromHours(1),
            created.OriginalFileName,
            cancellationToken).ConfigureAwait(false);

        return new PipelineVersionFileItemDto(
            created.Id,
            created.FileType,
            created.StorageObjectPath,
            created.OriginalFileName,
            url,
            created.CreatedAtUtc);
    }

    private static string? NormalizeFolderSegment(string segment)
    {
        var s = segment.Trim().Trim('/');
        if (s.Length == 0 || s.Contains("..", StringComparison.Ordinal) || s.Contains('/', StringComparison.Ordinal))
            return null;
        return s;
    }

    private static string SanitizeFileName(string name)
    {
        var s = string.IsNullOrWhiteSpace(name) ? "upload.bin" : Path.GetFileName(name.Trim());
        return s.Length > 512 ? s[..512] : s;
    }
}
