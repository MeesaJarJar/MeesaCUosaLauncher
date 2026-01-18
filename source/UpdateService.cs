namespace JarJarLauncher;

/// <summary>
/// Service for downloading and applying updates.
/// </summary>
public sealed class UpdateService
{
    private readonly string _baseDownloadUrl;
    private readonly string _clientPath;
    private readonly HttpClient _httpClient;

    public UpdateService(string baseDownloadUrl, string clientPath)
    {
        _baseDownloadUrl = baseDownloadUrl.TrimEnd('/');
        _clientPath = clientPath;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(LauncherConfig.Config.HttpTimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", LauncherConfig.Config.UserAgent);
    }

    /// <summary>
    /// Downloads and applies all files from the update check result.
    /// </summary>
    public async Task<bool> ApplyUpdateAsync(
        UpdateCheckResult updateResult, 
        string newVersion,
        IProgress<(string message, int percent)>? progress = null)
    {
        var allFiles = updateResult.FilesToAdd.Concat(updateResult.FilesToUpdate).ToList();
        var totalFiles = allFiles.Count;
        var completedFiles = 0;
        var failedFiles = new List<string>();

        foreach (var file in allFiles)
        {
            var percent = (int)((completedFiles / (double)totalFiles) * 100);
            progress?.Report(($"Downloading: {file.Filename}", percent));

            try
            {
                await DownloadFileAsync(file.Filename);
                completedFiles++;
            }
            catch (Exception ex)
            {
                failedFiles.Add($"{file.Filename}: {ex.Message}");
                progress?.Report(($"Failed: {file.Filename} - {ex.Message}", percent));
            }
        }

        // Write version file
        if (failedFiles.Count == 0)
        {
            var versionPath = Path.Combine(_clientPath, "version.txt");
            await File.WriteAllTextAsync(versionPath, newVersion);
            progress?.Report(("Update complete!", 100));
            return true;
        }
        else
        {
            progress?.Report(($"Update completed with {failedFiles.Count} errors", 100));
            return false;
        }
    }

    /// <summary>
    /// Downloads a single file from the update server.
    /// </summary>
    private async Task DownloadFileAsync(string relativePath)
    {
        var url = $"{_baseDownloadUrl}/{relativePath.Replace('\\', '/')}";
        var localPath = Path.Combine(_clientPath, relativePath);

        // Ensure directory exists
        var directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Download to temp file first
        var tempPath = localPath + ".tmp";

        try
        {
            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Create(tempPath);
            await stream.CopyToAsync(fileStream);

            // Replace original with downloaded file
            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }
            File.Move(tempPath, localPath);
        }
        finally
        {
            // Clean up temp file if it exists
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }
        }
    }
}
