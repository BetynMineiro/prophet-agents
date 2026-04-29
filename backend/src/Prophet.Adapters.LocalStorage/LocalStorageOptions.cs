namespace Prophet.Adapters.LocalStorage;

public sealed class LocalStorageOptions
{
    public const string SectionName = "Storage";
    public string Root { get; set; } = "prophet";
    public string BasePath { get; set; } = "./storage";
    public string ApiBaseUrl { get; set; } = "https://localhost:7017";
}
