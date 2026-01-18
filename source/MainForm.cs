using System.Diagnostics;

namespace JarJarLauncher;

public partial class MainForm : Form
{
    private readonly string _clientPath;
    private readonly UpdateChecker _updateChecker;
    private readonly BackupService _backupService;
    private readonly LauncherSettings _settings;
    private readonly ShardConfig _config;
    private UpdateCheckResult? _lastCheckResult;
    private PictureBox? _backgroundImage;

    private Label _titleLabel = null!;
    private Label _statusLabel = null!;
    private Label _versionLabel = null!;
    private Label _uoPathLabel = null!;
    private TextBox _uoPathTextBox = null!;
    private Button _browseUoButton = null!;
    private ProgressBar _progressBar = null!;
    private RichTextBox _logBox = null!;
    private Button _checkButton = null!;
    private Button _updateButton = null!;
    private Button _playButton = null!;

    public MainForm()
    {
        _clientPath = AppDomain.CurrentDomain.BaseDirectory;
        _config = LauncherConfig.Config;
        _backupService = new BackupService(_clientPath);
        _updateChecker = new UpdateChecker(_config.ManifestUrl, _clientPath, _backupService);
        _settings = LauncherSettings.Load();

        // Validate configuration
        var errors = _config.Validate();
        if (errors.Count > 0)
        {
            MessageBox.Show(
                $"Configuration errors found in shard_config.json:\n\n{string.Join("\n", errors)}\n\nPlease fix the configuration file.",
                "Configuration Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        InitializeComponents();
        
        // Try to auto-detect UO path if not set
        if (string.IsNullOrEmpty(_settings.UltimaOnlineDirectory))
        {
            var detected = LauncherSettings.DetectUODirectories();
            if (detected.Count > 0)
            {
                _settings.UltimaOnlineDirectory = detected[0];
                _uoPathTextBox.Text = _settings.UltimaOnlineDirectory;
                _settings.Save();
                Log($"Auto-detected UO directory: {_settings.UltimaOnlineDirectory}", Color.Cyan);
            }
        }
        else
        {
            _uoPathTextBox.Text = _settings.UltimaOnlineDirectory;
        }
        
        // Auto-check on startup if configured
        if (_config.AutoCheckUpdates)
        {
            _ = CheckForUpdatesAsync();
        }
    }

    private void InitializeComponents()
    {
        var theme = _config.UI.Theme;
        
        Text = _config.UI.Title;
        Size = new Size(_config.UI.WindowWidth, _config.UI.WindowHeight);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = UITheme.ParseColor(theme.BackgroundColor, Color.FromArgb(30, 30, 35));
        DoubleBuffered = true;

        // Background image (if configured)
        SetupBackgroundImage();

        // Title
        _titleLabel = new Label
        {
            Text = _config.ShardName,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = UITheme.ParseColor(theme.TitleColor, Color.FromArgb(100, 200, 255)),
            AutoSize = true,
            Location = new Point(20, 15),
            BackColor = Color.Transparent
        };
        Controls.Add(_titleLabel);

        int currentY = 55;

        // UO Path Section (optional)
        if (_config.UI.ShowUODirectoryField)
        {
            _uoPathLabel = new Label
            {
                Text = "Ultima Online Directory:",
                Font = new Font("Segoe UI", 10),
                ForeColor = UITheme.ParseColor(theme.TextColor, Color.White),
                AutoSize = true,
                Location = new Point(20, currentY),
                BackColor = Color.Transparent
            };
            Controls.Add(_uoPathLabel);

            currentY += 23;

            _uoPathTextBox = new TextBox
            {
                Location = new Point(20, currentY),
                Size = new Size(_config.UI.WindowWidth - 140, 25),
                Font = new Font("Segoe UI", 9),
                BackColor = UITheme.ParseColor(theme.TextBoxBackground, Color.FromArgb(50, 50, 55)),
                ForeColor = UITheme.ParseColor(theme.TextColor, Color.White),
                BorderStyle = BorderStyle.FixedSingle
            };
            _uoPathTextBox.TextChanged += (s, e) => ValidateUOPath();
            Controls.Add(_uoPathTextBox);

            _browseUoButton = new Button
            {
                Text = "Browse...",
                Location = new Point(_config.UI.WindowWidth - 110, currentY - 2),
                Size = new Size(80, 27),
                BackColor = UITheme.ParseColor(theme.ButtonBackground, Color.FromArgb(60, 60, 70)),
                ForeColor = UITheme.ParseColor(theme.ButtonTextColor, Color.White),
                FlatStyle = FlatStyle.Flat
            };
            _browseUoButton.Click += BrowseUOFolder;
            Controls.Add(_browseUoButton);

            currentY += 37;
        }

        // Version info (optional)
        if (_config.UI.ShowVersionLabel)
        {
            _versionLabel = new Label
            {
                Text = "Checking version...",
                Font = new Font("Segoe UI", 10),
                ForeColor = UITheme.ParseColor(theme.TextColor, Color.LightGray),
                AutoSize = true,
                Location = new Point(20, currentY),
                BackColor = Color.Transparent
            };
            Controls.Add(_versionLabel);
            currentY += 25;
        }

        // Status
        _statusLabel = new Label
        {
            Text = "Ready",
            Font = new Font("Segoe UI", 10),
            ForeColor = UITheme.ParseColor(theme.TextColor, Color.White),
            AutoSize = true,
            Location = new Point(20, currentY),
            BackColor = Color.Transparent
        };
        Controls.Add(_statusLabel);
        currentY += 30;

        // Progress bar
        _progressBar = new ProgressBar
        {
            Location = new Point(20, currentY),
            Size = new Size(_config.UI.WindowWidth - 50, 25),
            Style = ProgressBarStyle.Continuous
        };
        Controls.Add(_progressBar);
        currentY += 35;

        // Log box
        var logHeight = _config.UI.WindowHeight - currentY - 100;
        _logBox = new RichTextBox
        {
            Location = new Point(20, currentY),
            Size = new Size(_config.UI.WindowWidth - 50, logHeight),
            ReadOnly = true,
            BackColor = UITheme.ParseColor(theme.LogBackground, Color.FromArgb(20, 20, 25)),
            ForeColor = UITheme.ParseColor(theme.LogTextColor, Color.LightGreen),
            Font = new Font("Consolas", 9),
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(_logBox);
        currentY += logHeight + 10;

        // Buttons panel
        var buttonY = _config.UI.WindowHeight - 90;

        _checkButton = new Button
        {
            Text = _config.UI.CheckButtonText,
            Location = new Point(20, buttonY),
            Size = new Size(140, 35),
            BackColor = UITheme.ParseColor(theme.ButtonBackground, Color.FromArgb(60, 60, 70)),
            ForeColor = UITheme.ParseColor(theme.ButtonTextColor, Color.White),
            FlatStyle = FlatStyle.Flat
        };
        _checkButton.Click += async (s, e) => await CheckForUpdatesAsync();
        Controls.Add(_checkButton);

        _updateButton = new Button
        {
            Text = _config.UI.UpdateButtonText,
            Location = new Point(170, buttonY),
            Size = new Size(140, 35),
            BackColor = UITheme.ParseColor(theme.UpdateButtonBackground, Color.FromArgb(70, 130, 70)),
            ForeColor = UITheme.ParseColor(theme.ButtonTextColor, Color.White),
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        _updateButton.Click += async (s, e) => await PerformUpdateAsync();
        Controls.Add(_updateButton);

        _playButton = new Button
        {
            Text = _config.UI.PlayButtonText,
            Location = new Point(_config.UI.WindowWidth - 160, buttonY),
            Size = new Size(140, 35),
            BackColor = UITheme.ParseColor(theme.PlayButtonBackground, Color.FromArgb(50, 120, 200)),
            ForeColor = UITheme.ParseColor(theme.ButtonTextColor, Color.White),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11, FontStyle.Bold)
        };
        _playButton.Click += (s, e) => LaunchGame();
        Controls.Add(_playButton);

        // Log welcome message
        Log($"Welcome to {_config.ShardName}!", Color.Cyan);
        if (!string.IsNullOrWhiteSpace(_config.ShardDescription))
        {
            Log(_config.ShardDescription, Color.Gray);
        }

        // Ensure background image is at the very back
        if (_backgroundImage != null)
        {
            _backgroundImage.SendToBack();
        }
    }

    private void SetupBackgroundImage()
    {
        // Always look for background.png in the launcher folder
        var imagePath = Path.Combine(_clientPath, "background.png");
        
        if (!File.Exists(imagePath))
            return;

        try
        {
            var originalImage = Image.FromFile(imagePath);
            
            // Apply opacity
            var finalImage = ApplyOpacity(originalImage, _config.UI.BackgroundImageOpacity);

            _backgroundImage = new PictureBox
            {
                Image = finalImage,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(0, 0),
                Size = ClientSize
            };

            Controls.Add(_backgroundImage);
            _backgroundImage.SendToBack();
        }
        catch (Exception ex)
        {
            // Log error but don't crash - just skip background image
            System.Diagnostics.Debug.WriteLine($"Failed to load background image: {ex.Message}");
        }
    }

    private static Image ApplyOpacity(Image image, double opacity)
    {
        if (opacity >= 1.0)
            return image;

        opacity = Math.Clamp(opacity, 0.0, 1.0);

        var bmp = new Bitmap(image.Width, image.Height);
        using (var g = Graphics.FromImage(bmp))
        {
            var colorMatrix = new System.Drawing.Imaging.ColorMatrix
            {
                Matrix33 = (float)opacity
            };

            var attributes = new System.Drawing.Imaging.ImageAttributes();
            attributes.SetColorMatrix(colorMatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);

            g.DrawImage(image,
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                0, 0, image.Width, image.Height,
                GraphicsUnit.Pixel,
                attributes);
        }

        return bmp;
    }

    private void BrowseUOFolder(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select your Ultima Online installation folder",
            ShowNewFolderButton = false,
            UseDescriptionForTitle = true
        };

        if (!string.IsNullOrEmpty(_settings.UltimaOnlineDirectory))
        {
            dialog.InitialDirectory = _settings.UltimaOnlineDirectory;
        }

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _uoPathTextBox.Text = dialog.SelectedPath;
            ValidateUOPath();
        }
    }

    private void ValidateUOPath()
    {
        var path = _uoPathTextBox.Text;
        var theme = _config.UI.Theme;
        
        if (LauncherSettings.IsValidUODirectory(path))
        {
            _uoPathTextBox.BackColor = UITheme.ParseColor(theme.TextBoxValidBackground, Color.FromArgb(40, 70, 40));
            _settings.UltimaOnlineDirectory = path;
            _settings.Save();
            Log($"UO directory set: {path}", Color.LightGreen);
        }
        else if (!string.IsNullOrWhiteSpace(path))
        {
            _uoPathTextBox.BackColor = UITheme.ParseColor(theme.TextBoxInvalidBackground, Color.FromArgb(70, 40, 40));
            Log($"Invalid UO directory (missing art.mul or artLegacyMUL.uop)", Color.Orange);
        }
        else
        {
            _uoPathTextBox.BackColor = UITheme.ParseColor(theme.TextBoxBackground, Color.FromArgb(50, 50, 55));
        }
    }

    private void Log(string message, Color? color = null)
    {
        if (InvokeRequired)
        {
            Invoke(() => Log(message, color));
            return;
        }

        _logBox.SelectionStart = _logBox.TextLength;
        _logBox.SelectionColor = color ?? Color.LightGreen;
        _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
        _logBox.ScrollToCaret();
    }

    private void SetStatus(string status)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetStatus(status));
            return;
        }
        _statusLabel.Text = status;
    }

    private void SetProgress(int percent)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetProgress(percent));
            return;
        }
        _progressBar.Value = Math.Clamp(percent, 0, 100);
    }

    private void SetButtonsEnabled(bool checkEnabled, bool updateEnabled, bool playEnabled)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetButtonsEnabled(checkEnabled, updateEnabled, playEnabled));
            return;
        }
        _checkButton.Enabled = checkEnabled;
        _updateButton.Enabled = updateEnabled;
        _playButton.Enabled = playEnabled;
    }

    private async Task CheckForUpdatesAsync()
    {
        SetButtonsEnabled(false, false, false);
        SetProgress(0);
        Log("Checking for updates...", Color.Cyan);

        var progress = new Progress<string>(msg =>
        {
            SetStatus(msg);
            Log(msg);
        });

        _lastCheckResult = await _updateChecker.CheckForUpdatesAsync(progress);

        if (_lastCheckResult.Error != null)
        {
            Log($"Error: {_lastCheckResult.Error}", Color.Red);
            SetStatus("Error checking for updates");
            SetButtonsEnabled(true, false, true);
            return;
        }

        // Update version label
        if (_config.UI.ShowVersionLabel)
        {
            var localVer = _lastCheckResult.LocalVersion ?? "Not installed";
            var remoteVer = _lastCheckResult.RemoteVersion ?? "Unknown";
            _versionLabel.Text = $"Local: {localVer}  |  Latest: {remoteVer} ({_lastCheckResult.RemoteName})";
        }

        var theme = _config.UI.Theme;

        if (_lastCheckResult.UpdateAvailable)
        {
            Log($"Update available! {_lastCheckResult.FilesToAdd.Count} new files, {_lastCheckResult.FilesToUpdate.Count} changed files", Color.Yellow);
            SetStatus($"Update available: {_lastCheckResult.TotalChanges} files to update");
            SetButtonsEnabled(true, true, true);
            
            // Highlight update button
            _updateButton.BackColor = UITheme.ParseColor(theme.UpdateButtonHighlight, Color.FromArgb(200, 150, 50));
        }
        else
        {
            Log("All files are up to date!", Color.LightGreen);
            SetStatus("Up to date!");
            SetButtonsEnabled(true, false, true);
            _updateButton.BackColor = UITheme.ParseColor(theme.UpdateButtonBackground, Color.FromArgb(70, 130, 70));
        }

        SetProgress(100);
    }

    private async Task PerformUpdateAsync()
    {
        if (_lastCheckResult == null || !_lastCheckResult.UpdateAvailable)
            return;

        SetButtonsEnabled(false, false, false);
        SetProgress(0);

        try
        {
            // Create backup first
            Log("Creating backup before update...", Color.Cyan);
            var backupProgress = new Progress<string>(msg => Log(msg));
            var backupPath = await _backupService.CreateBackupAsync(backupProgress);
            Log($"Backup created: {Path.GetFileName(backupPath)}", Color.LightGreen);

            // Cleanup old backups
            _backupService.CleanupOldBackups(_config.BackupsToKeep);

            // Apply update
            Log("Downloading and applying updates...", Color.Cyan);
            var updateService = new UpdateService(_config.DownloadBaseUrl, _clientPath);
            
            var updateProgress = new Progress<(string message, int percent)>(data =>
            {
                SetStatus(data.message);
                SetProgress(data.percent);
                Log(data.message);
            });

            var success = await updateService.ApplyUpdateAsync(
                _lastCheckResult, 
                _lastCheckResult.RemoteVersion ?? "unknown",
                updateProgress);

            if (success)
            {
                Log("Update completed successfully!", Color.LightGreen);
                SetStatus("Update complete!");
                _lastCheckResult = null;
                _updateButton.Enabled = false;
                var theme = _config.UI.Theme;
                _updateButton.BackColor = UITheme.ParseColor(theme.UpdateButtonBackground, Color.FromArgb(70, 130, 70));
                
                // Re-check to confirm
                await CheckForUpdatesAsync();
            }
            else
            {
                Log("Update completed with some errors. Check the log above.", Color.Orange);
                SetStatus("Update completed with errors");
            }
        }
        catch (Exception ex)
        {
            Log($"Update failed: {ex.Message}", Color.Red);
            SetStatus("Update failed!");
        }
        finally
        {
            SetButtonsEnabled(true, _lastCheckResult?.UpdateAvailable ?? false, true);
        }
    }

    private void LaunchGame()
    {
        // Validate UO path first if field is shown
        if (_config.UI.ShowUODirectoryField && !LauncherSettings.IsValidUODirectory(_settings.UltimaOnlineDirectory))
        {
            Log("Please select a valid Ultima Online directory first!", Color.Red);
            MessageBox.Show(
                "Please select your Ultima Online installation folder before launching.\n\n" +
                "The folder should contain art.mul/artidx.mul or artLegacyMUL.uop files.",
                "UO Directory Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var workingDir = string.IsNullOrWhiteSpace(_config.ClientWorkingDirectory) 
            ? _clientPath 
            : Path.Combine(_clientPath, _config.ClientWorkingDirectory);

        var exePath = Path.Combine(workingDir, _config.ClientExecutable);

        if (!File.Exists(exePath))
        {
            Log($"Client not found: {_config.ClientExecutable}", Color.Red);
            MessageBox.Show(
                $"Client executable not found!\n\nExpected: {exePath}\n\nPlease run an update first.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Log($"Launching {_config.ClientExecutable}...", Color.Cyan);

        try
        {
            // Build launch arguments with variable substitution
            var args = _config.BuildLaunchArguments(_settings.UltimaOnlineDirectory ?? string.Empty);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                WorkingDirectory = workingDir,
                UseShellExecute = false
            };

            Log($"Launch args: {args}", Color.Gray);
            Process.Start(startInfo);
            
            Log("Game launched!", Color.LightGreen);
            
            // Close launcher if configured
            if (_config.CloseLauncherOnGameStart)
            {
                Log("Closing launcher...", Color.Gray);
                Task.Delay(1000).ContinueWith(_ => 
                {
                    Invoke(() => Close());
                });
            }
        }
        catch (Exception ex)
        {
            Log($"Failed to launch: {ex.Message}", Color.Red);
            MessageBox.Show(
                $"Failed to launch the game!\n\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
