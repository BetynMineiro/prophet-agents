namespace Prophet.Application.Options;

/// <summary>Configuration for storage (e.g. Firebase/GCS). Default path for any upload: root/{ownerId}/{productName}/{assetType}/{fileName} (e.g. genesis/{userID}/{ProductName}/avatars/{fileName}).</summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Root segment of the path. Default: genesis.</summary>
    public string Root { get; set; } = "genesis";

    /// <summary>Optional fallback owner id (e.g. for delete by prefix). Upload path uses ownerId per call (e.g. userId).</summary>
    public string ClientId { get; set; } = "default";

    /// <summary>Bucket name (Firebase Storage / GCS bucket).</summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>Signed URL validity in days (max 7 for GCS). Default: 7.</summary>
    public int SignedUrlExpirationDays { get; set; } = 7;
}
