using System.Text.Json;
using System.Text.Json.Serialization;

namespace JarJarLauncher;

/// <summary>
/// Shard-specific configuration loaded from shard_config.json.
/// This file should be customized by shard developers.
/// </summary>
public sealed class ShardConfig
{
    // Shard Identity
    [JsonPropertyName("shard_name")]
    public string ShardName { get; set; } = "My Ultima Online Shard";

    [JsonPropertyName("shard_description")]
    public string ShardDescription { get; set; } = "A custom UO shard";

    // Update Sources
    [JsonPropertyName("manifest_url")]
    public string ManifestUrl { get; set; } = "https://raw.githubusercontent.com/YourUsername/YourRepo/main/manifest.xml";

    [JsonPropertyName("download_base_url")]
    public string DownloadBaseUrl { get; set; } = "https://raw.githubusercontent.com/YourUsername/YourRepo/main";

    // Client Configuration
    [JsonPropertyName("client_executable")]
    public string ClientExecutable { get; set; } = "ClassicUO.exe";

    [JsonPropertyName("client_working_directory")]
    public string? ClientWorkingDirectory { get; set; } = null;

    // Server Connection
    [JsonPropertyName("server_address")]
    public string ServerAddress { get; set; } = "127.0.0.1";

    [JsonPropertyName("server_port")]
    public int ServerPort { get; set; } = 2593;

    // Launch Arguments
    [JsonPropertyName("base_arguments")]
    public List<LaunchArgument> BaseArguments { get; set; } = new()
    {
        new LaunchArgument { Key = "-ip", Value = "{SERVER_ADDRESS}" },
        new LaunchArgument { Key = "-port", Value = "{SERVER_PORT}" },
        new LaunchArgument { Key = "-ultimaonlinedirectory", Value = "{UO_DIRECTORY}", IsQuoted = true }
    };

    [JsonPropertyName("custom_arguments")]
    public List<LaunchArgument> CustomArguments { get; set; } = new();

    // UI Customization
    [JsonPropertyName("ui_settings")]
    public UISettings UI { get; set; } = new();

    // Advanced Settings
    [JsonPropertyName("backups_to_keep")]
    public int BackupsToKeep { get; set; } = 5;

    [JsonPropertyName("preserved_paths")]
    public List<string> PreservedPaths { get; set; } = new()
    {
        "Data",
        "Profiles",
        "settings.json",
        "backup"
    };

    [JsonPropertyName("auto_check_updates")]
    public bool AutoCheckUpdates { get; set; } = true;

    [JsonPropertyName("close_launcher_on_game_start")]
    public bool CloseLauncherOnGameStart { get; set; } = true;

    [JsonPropertyName("http_timeout_seconds")]
    public int HttpTimeoutSeconds { get; set; } = 300;

    [JsonPropertyName("user_agent")]
    public string UserAgent { get; set; } = "JarJarLauncher/1.0";

    // Internal settings file name
    [JsonIgnore]
    public string SettingsFile => "launcher_settings.json";

    private static string ConfigPath => Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "shard_config.json");

    /// <summary>
    /// Loads shard configuration from shard_config.json.
    /// Creates a default config if none exists.
    /// </summary>
    public static ShardConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                var config = JsonSerializer.Deserialize<ShardConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });
                
                return config ?? CreateDefault();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error loading shard_config.json:\n\n{ex.Message}\n\nUsing default configuration.",
                "Configuration Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        return CreateDefault();
    }

    /// <summary>
    /// Creates and saves a default configuration file.
    /// </summary>
    private static ShardConfig CreateDefault()
    {
        var config = new ShardConfig();
        config.Save();
        return config;
    }

    /// <summary>
    /// Saves the current configuration to shard_config.json.
    /// </summary>
    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error saving shard_config.json:\n\n{ex.Message}",
                "Configuration Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Builds the complete launch arguments string with variable substitution.
    /// </summary>
    public string BuildLaunchArguments(string uoDirectory)
    {
        var allArgs = BaseArguments.Concat(CustomArguments).ToList();
        var argParts = new List<string>();

        foreach (var arg in allArgs)
        {
            var value = arg.Value
                .Replace("{SERVER_ADDRESS}", ServerAddress)
                .Replace("{SERVER_PORT}", ServerPort.ToString())
                .Replace("{UO_DIRECTORY}", uoDirectory);

            if (string.IsNullOrEmpty(arg.Key))
            {
                // Flag-only argument (no key-value pair)
                argParts.Add(value);
            }
            else
            {
                // Key-value pair
                if (arg.IsQuoted && !string.IsNullOrEmpty(value))
                {
                    argParts.Add($"{arg.Key} \"{value}\"");
                }
                else
                {
                    argParts.Add($"{arg.Key} {value}");
                }
            }
        }

        return string.Join(" ", argParts);
    }

    /// <summary>
    /// Validates the configuration and returns any errors.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ShardName))
            errors.Add("Shard name is required");

        if (string.IsNullOrWhiteSpace(ManifestUrl))
            errors.Add("Manifest URL is required");

        if (string.IsNullOrWhiteSpace(DownloadBaseUrl))
            errors.Add("Download base URL is required");

        if (string.IsNullOrWhiteSpace(ClientExecutable))
            errors.Add("Client executable name is required");

        if (string.IsNullOrWhiteSpace(ServerAddress))
            errors.Add("Server address is required");

        if (ServerPort <= 0 || ServerPort > 65535)
            errors.Add("Server port must be between 1 and 65535");

        if (BackupsToKeep < 0)
            errors.Add("Backups to keep must be 0 or greater");

        if (HttpTimeoutSeconds < 30)
            errors.Add("HTTP timeout must be at least 30 seconds");

        return errors;
    }
}
