namespace Prophet.Adapters.Postgres.Persistence;

internal static class DesignTimeAppSettings
{
    /// <summary>Finds appsettings.json when dotnet ef runs from repo root or src.</summary>
    internal static string ResolveBasePath(string apiProjectFolderName)
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var i = 0; i < 8 && dir != null; i++)
        {
            var apiDir = Path.Combine(dir.FullName, apiProjectFolderName);
            if (File.Exists(Path.Combine(apiDir, "appsettings.json")))
                return apiDir;
            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
