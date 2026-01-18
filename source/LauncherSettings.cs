using System.Text.Json;

namespace JarJarLauncher;

/// <summary>
/// Stores user settings for the launcher.
/// </summary>
public sealed class LauncherSettings
{
    public string? UltimaOnlineDirectory { get; set; }
    public string? LastVersion { get; set; }
    
    private static string SettingsPath => Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        LauncherConfig.Config.SettingsFile);

    public static LauncherSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<LauncherSettings>(json) ?? new LauncherSettings();
            }
        }
        catch
        {
            // Ignore errors, return default settings
        }
        
        return new LauncherSettings();
    }

    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    /// <summary>
    /// Validates that the UO directory contains required files.
    /// </summary>
    public static bool IsValidUODirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return false;

        // Check for common UO files
        var requiredFiles = new[] { "art.mul", "artidx.mul" };
        var optionalFiles = new[] { "artLegacyMUL.uop", "client.exe", "ClassicUO.exe" };

        // Must have either MUL files or UOP files
        bool hasMulFiles = requiredFiles.All(f => File.Exists(Path.Combine(path, f)));
        bool hasUopFiles = File.Exists(Path.Combine(path, "artLegacyMUL.uop"));

        return hasMulFiles || hasUopFiles;
    }

    /// <summary>
    /// Tries to auto-detect UO installation directories.
    /// </summary>
    public static List<string> DetectUODirectories()
    {
        var found = new List<string>();

        // Common UO installation paths
        var commonPaths = new[]
        {
            @"C:\Program Files (x86)\Electronic Arts\Ultima Online Classic",
            @"C:\Program Files\Electronic Arts\Ultima Online Classic",
            @"C:\Ultima Online",
            @"C:\UO",
            @"C:\Games\Ultima Online",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Ultima Online"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Ultima Online"),
        };

        foreach (var path in commonPaths)
        {
            if (IsValidUODirectory(path))
            {
                found.Add(path);
            }
        }

        return found;
    }
}
