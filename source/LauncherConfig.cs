namespace JarJarLauncher;

/// <summary>
/// Configuration accessor that loads from ShardConfig.
/// Provides backward compatibility with the old LauncherConfig structure.
/// </summary>
public static class LauncherConfig
{
    private static ShardConfig? _config;
    
    public static ShardConfig Config
    {
        get
        {
            _config ??= ShardConfig.Load();
            return _config;
        }
    }

    // Backward compatibility properties
    public static string ManifestUrl => Config.ManifestUrl;
    public static string DownloadBaseUrl => Config.DownloadBaseUrl;
    public static string ClientExe => Config.ClientExecutable;
    public static string SettingsFile => Config.SettingsFile;
    public static int BackupsToKeep => Config.BackupsToKeep;
    public static string UserAgent => Config.UserAgent;
    
    /// <summary>
    /// Forces a reload of the configuration from disk.
    /// </summary>
    public static void Reload()
    {
        _config = ShardConfig.Load();
    }
}
