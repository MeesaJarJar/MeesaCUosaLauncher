using System.Security.Cryptography;

namespace JarJarLauncher;

/// <summary>
/// Result of comparing local files against manifest.
/// </summary>
public sealed class UpdateCheckResult
{
    public bool UpdateAvailable { get; set; }
    public string? LocalVersion { get; set; }
    public string? RemoteVersion { get; set; }
    public string? RemoteName { get; set; }
    public List<ManifestFile> FilesToUpdate { get; set; } = new();
    public List<ManifestFile> FilesToAdd { get; set; } = new();
    public List<string> FilesToDelete { get; set; } = new();
    public string? Error { get; set; }

    public int TotalChanges => FilesToUpdate.Count + FilesToAdd.Count + FilesToDelete.Count;
}

/// <summary>
/// Service for checking updates against a remote manifest.
/// </summary>
public sealed class UpdateChecker
{
    private readonly string _manifestUrl;
    private readonly string _clientPath;
    private readonly HttpClient _httpClient;
    private readonly BackupService _backupService;

    public UpdateChecker(string manifestUrl, string clientPath, BackupService backupService)
    {
        _manifestUrl = manifestUrl;
        _clientPath = clientPath;
        _backupService = backupService;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(LauncherConfig.Config.HttpTimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", LauncherConfig.Config.UserAgent);
    }

    /// <summary>
    /// Fetches the manifest from the remote server.
    /// </summary>
    public async Task<(Manifest? manifest, string? error)> FetchManifestAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_manifestUrl);

            if (!response.IsSuccessStatusCode)
            {
                return (null, $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}. URL: {_manifestUrl}");
            }

            var xml = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(xml))
            {
                return (null, "Empty response from server");
            }

            var manifest = Manifest.Parse(xml);
            return (manifest, null);
        }
        catch (HttpRequestException ex)
        {
            return (null, $"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (null, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks local files against the remote manifest.
    /// </summary>
    public async Task<UpdateCheckResult> CheckForUpdatesAsync(IProgress<string>? progress = null)
    {
        var result = new UpdateCheckResult();

        progress?.Report($"Fetching manifest from {_manifestUrl}...");

        var (manifest, fetchError) = await FetchManifestAsync();
        
        if (fetchError != null)
        {
            result.Error = $"Failed to fetch manifest: {fetchError}";
            return result;
        }

        if (manifest == null || manifest.LatestRelease == null)
        {
            result.Error = "Could not parse manifest or no releases found in the XML.";
            return result;
        }

        var release = manifest.LatestRelease;
        result.RemoteVersion = release.Version;
        result.RemoteName = release.Name;

        progress?.Report($"Checking against version: {release.Name} ({release.Version})");

        // Read local version if exists
        var versionFile = Path.Combine(_clientPath, "version.txt");
        if (File.Exists(versionFile))
        {
            result.LocalVersion = File.ReadAllText(versionFile).Trim();
        }

        // Check each file in the manifest
        var totalFiles = release.Files.Count;
        var checkedFiles = 0;

        await Task.Run(() =>
        {
            using var md5 = MD5.Create();

            foreach (var manifestFile in release.Files)
            {
                // Skip preserved files
                if (_backupService.ShouldPreserve(manifestFile.Filename))
                {
                    checkedFiles++;
                    continue;
                }

                var localPath = Path.Combine(_clientPath, manifestFile.Filename);

                if (!File.Exists(localPath))
                {
                    result.FilesToAdd.Add(manifestFile);
                    progress?.Report($"  [NEW] {manifestFile.Filename} - file missing locally");
                }
                else
                {
                    var localHash = CalculateMD5(localPath, md5);
                    if (!string.Equals(localHash, manifestFile.Hash, StringComparison.OrdinalIgnoreCase))
                    {
                        result.FilesToUpdate.Add(manifestFile);
                        progress?.Report($"  [CHANGED] {manifestFile.Filename}");
                        progress?.Report($"      Local:  {localHash}");
                        progress?.Report($"      Remote: {manifestFile.Hash}");
                    }
                }

                checkedFiles++;
                if (checkedFiles % 20 == 0)
                {
                    progress?.Report($"Checking files: {checkedFiles}/{totalFiles}");
                }
            }
        });

        result.UpdateAvailable = result.TotalChanges > 0;

        if (result.UpdateAvailable)
        {
            progress?.Report($"Update available: {result.FilesToAdd.Count} new, {result.FilesToUpdate.Count} changed");
        }
        else
        {
            progress?.Report("All files are up to date!");
        }

        return result;
    }

    private static string CalculateMD5(string filePath, MD5 md5)
    {
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
