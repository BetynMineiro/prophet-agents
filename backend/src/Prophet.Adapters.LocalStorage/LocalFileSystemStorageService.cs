using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prophet.Application.Interfaces.Storage;

namespace Prophet.Adapters.LocalStorage;

public sealed class LocalFileSystemStorageService(
    IOptions<LocalStorageOptions> options,
    ILogger<LocalFileSystemStorageService> logger) : IStorageService
{
    private readonly LocalStorageOptions _options = options.Value;

    public async Task<string> UploadAsync(
        string root,
        string ownerId,
        string product,
        string assetType,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var relativePath = Path.Combine(root, ownerId, product, assetType, fileName);
        var fullPath = Path.Combine(_options.BasePath, relativePath);
        var dir = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(dir);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

        logger.LogDebug("Stored file at {Path}", fullPath);
        return relativePath.Replace('\\', '/');
    }

    public Task<string?> GetSignedUrlAsync(
        string? objectPath,
        TimeSpan expiration,
        string? downloadAsFileName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
            return Task.FromResult<string?>(null);

        var url = $"{_options.ApiBaseUrl.TrimEnd('/')}/v1/prophet/files/{objectPath}";
        return Task.FromResult<string?>(url);
    }

    public async Task<byte[]?> ReadObjectAsync(string objectPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
            return null;

        var fullPath = Path.Combine(_options.BasePath, objectPath);
        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(fullPath, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(
        string root,
        string ownerId,
        string? product = null,
        string? assetType = null,
        string? fileName = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var segments = new List<string> { _options.BasePath, root, ownerId };
            if (!string.IsNullOrWhiteSpace(product)) segments.Add(product);
            if (!string.IsNullOrWhiteSpace(assetType)) segments.Add(assetType);
            segments.Add(fileName);

            var fullPath = Path.Combine(segments.ToArray());
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        else
        {
            var segments = new List<string> { _options.BasePath, root, ownerId };
            if (!string.IsNullOrWhiteSpace(product)) segments.Add(product);
            if (!string.IsNullOrWhiteSpace(assetType)) segments.Add(assetType);

            var dir = Path.Combine(segments.ToArray());
            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                    File.Delete(file);
            }
        }

        return Task.CompletedTask;
    }

    public Task DeleteObjectAsync(string objectPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
            return Task.CompletedTask;

        var fullPath = Path.Combine(_options.BasePath, objectPath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
