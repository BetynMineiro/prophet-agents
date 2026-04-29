namespace Prophet.Application.Interfaces.Storage;

/// <summary>
/// Storage abstraction for file upload (returns object path for persistence) and delete by prefix.
/// Default path for any upload: {root}/{ownerId}/{productName}/{assetType}/{fileName}
/// (e.g. genesis/{userID}/{ProductName}/avatars/{fileName}). Pass userId as ownerId for user-owned content.
/// Use GetSignedUrlAsync to obtain a time-limited display URL.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Uploads content and returns the object path to persist.
    /// Default path: root/ownerId/product/assetType/fileName (e.g. genesis/{userID}/{ProductName}/avatars/{fileName}).
    /// Use GetSignedUrlAsync to get a display URL with expiration.
    /// </summary>
    Task<string> UploadAsync(
        string root,
        string ownerId,
        string product,
        string assetType,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a signed URL for the given object path, valid for the specified duration (GCS max 7 days).
    /// Returns null if path is null/empty or storage does not support signing.
    /// When <paramref name="downloadAsFileName"/> is set (GCS/Firebase), the URL includes <c>response-content-disposition=attachment</c> so browsers save the file instead of displaying inline.
    /// </summary>
    Task<string?> GetSignedUrlAsync(
        string? objectPath,
        TimeSpan expiration,
        string? downloadAsFileName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads object bytes from storage by full object path (server-side; avoids browser CORS on signed GET to GCS).
    /// Returns null if path is empty, bucket is not configured, or the object does not exist.
    /// </summary>
    Task<byte[]?> ReadObjectAsync(string objectPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes by path, using the same segments as upload. If <paramref name="fileName"/> is set, deletes that single object;
    /// otherwise deletes all objects under the prefix (e.g. whole "folder"). Segments after <paramref name="ownerId"/> are optional.
    /// </summary>
    /// <param name="fileName">Optional. When null, deletes everything under root/ownerId/product/assetType/.</param>
    Task DeleteAsync(
        string root,
        string ownerId,
        string? product = null,
        string? assetType = null,
        string? fileName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a single object by its full path (e.g. from persisted AvatarUrl). Use when you have the stored path.
    /// </summary>
    Task DeleteObjectAsync(string objectPath, CancellationToken cancellationToken = default);
}
