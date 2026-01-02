namespace DiscordEconomyBot.Services;

public static class VersionService
{
    private static string? _cachedVersion;
    private static string? _cachedCommitHash;

    public static string GetVersion()
    {
        if (_cachedVersion != null)
            return _cachedVersion;

        var commitHash = GetCommitHash();
        _cachedVersion = string.IsNullOrEmpty(commitHash) 
            ? "1.0.0" 
            : $"1.0.0-{commitHash}";

        return _cachedVersion;
    }

    public static string GetCommitHash()
    {
        if (_cachedCommitHash != null)
            return _cachedCommitHash;

        try
        {
            // Read commit hash from version.txt file created during build
            var versionFile = Path.Combine(AppContext.BaseDirectory, "version.txt");
            
            if (File.Exists(versionFile))
            {
                var content = File.ReadAllText(versionFile).Trim();
                if (!string.IsNullOrEmpty(content))
                {
                    _cachedCommitHash = content;
                    return content;
                }
            }
        }
        catch
        {
            // File not found or cannot be read
        }

        _cachedCommitHash = "unknown";
        return "unknown";
    }

    public static string GetFullVersionInfo()
    {
        var version = GetVersion();
        var commitHash = GetCommitHash();
        
        return commitHash == "unknown"
            ? $"v{version}"
            : $"v{version} (commit: {commitHash})";
    }
}
