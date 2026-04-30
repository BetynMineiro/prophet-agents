namespace Prophet.Application.Options;

/// <summary>Configuration for in-memory storage. Files are held in-process; served via GET /v1/prophet/files/{path}.</summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Root segment prepended to all object paths. Default: prophet.</summary>
    public string Root { get; set; } = "prophet";

    /// <summary>Base URL of the API used to build file serve URLs (e.g. https://localhost:7017).</summary>
    public string ApiBaseUrl { get; set; } = "https://localhost:7017";
}
