using System.IO.Compression;

namespace JarJarLauncher;

/// <summary>
/// Service for creating backups of the client files before updates.
/// </summary>
public sealed class BackupService
{
    private readonly string _clientPath;
    private readonly string _backupFolder;
    private readonly List<string> _preservedPaths;

    public BackupService(string clientPath, List<string>? preservedPaths = null)
    {
        _clientPath = clientPath;
        _backupFolder = Path.Combine(clientPath, "backup");
        _preservedPaths = preservedPaths ?? LauncherConfig.Config.PreservedPaths;
    }

    /// <summary>
    /// Creates a backup of all client files (except preserved/excluded) into a timestamped zip.
    /// </summary>
    public async Task<string> CreateBackupAsync(IProgress<string>? progress = null)
    {
        // Ensure backup directory exists
        Directory.CreateDirectory(_backupFolder);

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var backupFileName = $"backup_{timestamp}.zip";
        var backupPath = Path.Combine(_backupFolder, backupFileName);

        progress?.Report($"Creating backup: {backupFileName}");

        await Task.Run(() =>
        {
            using var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create);

            var files = Directory.GetFiles(_clientPath, "*.*", SearchOption.AllDirectories);
            var totalFiles = files.Length;
            var processedFiles = 0;

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(_clientPath, file);

                // Skip excluded paths
                if (ShouldExclude(relativePath))
                {
                    processedFiles++;
                    continue;
                }

                try
                {
                    archive.CreateEntryFromFile(file, relativePath, CompressionLevel.Optimal);
                    processedFiles++;

                    if (processedFiles % 10 == 0)
                    {
                        progress?.Report($"Backing up: {processedFiles}/{totalFiles} files");
                    }
                }
                catch (Exception ex)
                {
                    progress?.Report($"Warning: Could not backup {relativePath}: {ex.Message}");
                }
            }
        });

        progress?.Report($"Backup created: {backupFileName}");
        return backupPath;
    }

    /// <summary>
    /// Checks if a path should be excluded from backup.
    /// </summary>
    private bool ShouldExclude(string relativePath)
    {
        // Always exclude backup folder
        if (relativePath.StartsWith("backup", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Checks if a path should be preserved during updates (not replaced).
    /// </summary>
    public bool ShouldPreserve(string relativePath)
    {
        foreach (var preserved in _preservedPaths)
        {
            if (relativePath.StartsWith(preserved, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Cleans up old backups, keeping only the most recent N backups.
    /// </summary>
    public void CleanupOldBackups(int keepCount = 5)
    {
        if (!Directory.Exists(_backupFolder))
            return;

        var backups = Directory.GetFiles(_backupFolder, "backup_*.zip")
            .OrderByDescending(f => f)
            .Skip(keepCount)
            .ToList();

        foreach (var backup in backups)
        {
            try
            {
                File.Delete(backup);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
