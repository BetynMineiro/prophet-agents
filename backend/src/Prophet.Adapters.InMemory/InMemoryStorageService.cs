using System.Collections.Concurrent;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.Options;
using Microsoft.Extensions.Options;

namespace Prophet.Adapters.InMemory;

public class InMemoryStorageService(IOptions<StorageOptions> options) : IStorageService
{
    private readonly ConcurrentDictionary<string, byte[]> _store = new();
    private readonly StorageOptions _options = options.Value;

    public async Task<string> UploadAsync(
        string root, string ownerId, string product, string assetType, string fileName,
        Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var path = $"{root}/{ownerId}/{product}/{assetType}/{fileName}";
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, cancellationToken);
        _store[path] = ms.ToArray();
        return path;
    }

    public Task<string?> GetSignedUrlAsync(
        string? objectPath, TimeSpan expiration, string? downloadAsFileName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
            return Task.FromResult<string?>(null);
        var baseUrl = _options.ApiBaseUrl?.TrimEnd('/') ?? "https://localhost:7017";
        return Task.FromResult<string?>($"{baseUrl}/v1/prophet/files/{objectPath}");
    }

    public Task<byte[]?> ReadObjectAsync(string objectPath, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(objectPath, out var bytes);
        return Task.FromResult(bytes);
    }

    public Task DeleteAsync(
        string root, string ownerId, string? product = null, string? assetType = null,
        string? fileName = null, CancellationToken cancellationToken = default)
    {
        string prefix;
        if (fileName != null)
            prefix = $"{root}/{ownerId}/{product}/{assetType}/{fileName}";
        else if (assetType != null)
            prefix = $"{root}/{ownerId}/{product}/{assetType}/";
        else if (product != null)
            prefix = $"{root}/{ownerId}/{product}/";
        else
            prefix = $"{root}/{ownerId}/";

        foreach (var key in _store.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList())
            _store.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    public Task DeleteObjectAsync(string objectPath, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(objectPath, out _);
        return Task.CompletedTask;
    }
}
