# ?? JarJarLauncher

<div align="center">

![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
![ClassicUO](https://img.shields.io/badge/ClassicUO-Compatible-orange?style=for-the-badge)

**A fully configurable updater and launcher for Ultima Online shards**

*Built for ServUO • ClassicUO • Any UO Shard*

[Getting Started](#-getting-started) •
[Configuration](#-configuration) •
[For Shard Developers](#-for-shard-developers) •
[Screenshots](#-screenshots)

</div>

---

## ? Features

| Feature | Description |
|---------|-------------|
| ?? **Auto-Updates** | GitHub-based file updates with MD5 verification |
| ?? **Fully Customizable UI** | Colors, text, window size - all via JSON config |
| ?? **Automatic Backups** | Creates backups before every update |
| ?? **Flexible Launch Args** | Add any custom arguments for your client |
| ??? **File Preservation** | Protects user settings, profiles, and data |
| ?? **UO Auto-Detection** | Automatically finds UO installation directories |
| ? **Single EXE** | Just one file + config - easy distribution |

---

## ?? Getting Started

### For Players

1. **Download** the launcher from your shard's website
2. **Run** `JarJarLauncher.exe`
3. **Select** your Ultima Online installation folder (if prompted)
4. **Click** "Update Now" to download the latest client files
5. **Click** "PLAY" to launch the game!

### For Shard Developers

1. Download the latest release
2. Edit `shard_config.json` with your shard's details
3. Create a `manifest.xml` in your GitHub repository
4. Distribute the launcher to your players

---

## ?? Configuration

All configuration is done through a single `shard_config.json` file:

```json
{
  "shard_name": "My Awesome Shard",
  "shard_description": "Welcome to the best UO experience!",
  "manifest_url": "https://raw.githubusercontent.com/YourUser/YourRepo/main/manifest.xml",
  "download_base_url": "https://raw.githubusercontent.com/YourUser/YourRepo/main",
  "client_executable": "ClassicUO.exe",
  "server_address": "play.myawesomeshard.com",
  "server_port": 2593
}
```

### ?? Key Configuration Options

<details>
<summary><b>?? Server Connection</b></summary>

```json
{
  "server_address": "127.0.0.1",
  "server_port": 2593
}
```

</details>

<details>
<summary><b>?? Launch Arguments</b></summary>

Add custom launch arguments with variable substitution:

```json
{
  "custom_arguments": [
    {
      "key": "-debug",
      "value": "true",
      "is_quoted": false,
      "description": "Enable debug mode"
    },
    {
      "key": "-custompath",
      "value": "C:\\MyData",
      "is_quoted": true,
      "description": "Custom data path"
    }
  ]
}
```

**Available Variables:**
| Variable | Replaced With |
|----------|---------------|
| `{SERVER_ADDRESS}` | Your server IP/domain |
| `{SERVER_PORT}` | Your server port |
| `{UO_DIRECTORY}` | Player's UO installation path |

</details>

<details>
<summary><b>?? UI Theming</b></summary>

Fully customizable colors using hex values:

```json
{
  "ui_settings": {
    "title": "My Shard Launcher",
    "window_width": 720,
    "window_height": 580,
    "theme": {
      "background_color": "#1E1E23",
      "title_color": "#64C8FF",
      "text_color": "#FFFFFF",
      "play_button_background": "#3278C8",
      "update_button_background": "#468246"
    },
    "play_button_text": "ENTER BRITANNIA",
    "show_uo_directory_field": true,
    "show_version_label": true
  }
}
```

</details>

<details>
<summary><b>??? File Preservation</b></summary>

Protect files/folders from being overwritten during updates:

```json
{
  "preserved_paths": [
    "Data",
    "Profiles", 
    "settings.json",
    "backup"
  ]
}
```

</details>

---

## ????? For Shard Developers

### Creating Your Manifest

Create a `manifest.xml` file in your GitHub repository:

```xml
<?xml version="1.0" encoding="utf-8"?>
<releases>
  <release version="1.0.0" name="Initial Release">
    <files>
      <file filename="ClassicUO.exe" hash="abc123def456..." />
      <file filename="ClassicUO.dll" hash="789ghi012jkl..." />
      <file filename="Data/config.xml" hash="mno345pqr678..." />
    </files>
  </release>
</releases>
```

### Generating MD5 Hashes

**PowerShell:**
```powershell
Get-FileHash -Algorithm MD5 "ClassicUO.exe" | Select-Object -ExpandProperty Hash
```

**Bash:**
```bash
md5sum ClassicUO.exe
```

### Repository Structure

```
YourRepo/
??? manifest.xml           # File manifest with hashes
??? ClassicUO.exe          # Client executable
??? ClassicUO.dll          # Client libraries
??? Data/                  # Data files
?   ??? ...
??? [other client files]
```

### Distribution Package

Give your players:
```
MyShard/
??? JarJarLauncher.exe     # The launcher
??? shard_config.json      # Your configuration
??? (client files download automatically)
```

---

## ?? Theme Examples

<details>
<summary><b>?? Dark Blue (Default)</b></summary>

```json
"theme": {
  "background_color": "#1E1E23",
  "title_color": "#64C8FF",
  "text_color": "#FFFFFF",
  "play_button_background": "#3278C8"
}
```

</details>

<details>
<summary><b>?? Medieval Gold</b></summary>

```json
"theme": {
  "background_color": "#2C1810",
  "title_color": "#D4AF37",
  "text_color": "#E8D7C3",
  "play_button_background": "#8B4513"
}
```

</details>

<details>
<summary><b>?? Cyberpunk Neon</b></summary>

```json
"theme": {
  "background_color": "#0A0A0F",
  "title_color": "#00FFFF",
  "text_color": "#00FF88",
  "play_button_background": "#FF006E"
}
```

</details>

<details>
<summary><b>?? Forest Green</b></summary>

```json
"theme": {
  "background_color": "#1B2E1F",
  "title_color": "#7CB342",
  "text_color": "#C5E1A5",
  "play_button_background": "#558B2F"
}
```

</details>

---

## ?? Screenshots

<div align="center">

*Screenshots coming soon!*

</div>

---

## ?? Building from Source

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Windows 10/11

### Build Commands

```bash
# Clone the repository
git clone https://github.com/YourUsername/JarJarLauncher.git
cd JarJarLauncher

# Build Debug
dotnet build

# Build Release
dotnet build -c Release

# Publish single-file executable
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

---

## ?? Project Structure

```
JarJarLauncher/
??? ?? Program.cs              # Application entry point
??? ?? MainForm.cs             # Main UI form
??? ?? ShardConfig.cs          # Configuration model
??? ?? ConfigModels.cs         # UI/Theme/Argument models
??? ?? LauncherConfig.cs       # Config accessor
??? ?? LauncherSettings.cs     # User settings persistence
??? ?? UpdateChecker.cs        # Update detection service
??? ?? UpdateService.cs        # File download service
??? ?? BackupService.cs        # Backup management
??? ?? ManifestModels.cs       # XML manifest parsing
??? ?? shard_config.json       # Default configuration
??? ?? Examples/               # Example configurations
    ??? shard_config_simple.json
    ??? shard_config_advanced.json
    ??? shard_config_jarjar_full.json
```

---

## ? Troubleshooting

<details>
<summary><b>? "Configuration errors found"</b></summary>

- Check `shard_config.json` for JSON syntax errors
- Validate at [jsonlint.com](https://jsonlint.com)
- Ensure all required fields are filled

</details>

<details>
<summary><b>? "Failed to fetch manifest"</b></summary>

- Verify your `manifest_url` is correct
- Ensure the URL uses `raw.githubusercontent.com`
- Check that the file exists and is publicly accessible

</details>

<details>
<summary><b>? "Client not found"</b></summary>

- Run "Update Now" to download client files
- Verify `client_executable` matches your actual file name
- Check `client_working_directory` if using a subdirectory

</details>

<details>
<summary><b>? "Invalid UO directory"</b></summary>

- Select a folder containing `art.mul` or `artLegacyMUL.uop`
- This should be your Ultima Online installation folder
- Common locations: `C:\Program Files (x86)\Electronic Arts\Ultima Online Classic`

</details>

---

## ?? License

This project is provided as-is for use by Ultima Online shard developers and players.

---

## ?? Acknowledgments

- [ClassicUO](https://github.com/ClassicUO/ClassicUO) - The amazing open-source UO client
- [ServUO](https://github.com/ServUO/ServUO) - The powerful UO server emulator
- The entire Ultima Online community ??

---

<div align="center">

**Made with ?? for the Ultima Online Community**

*JarJarLauncher - Meesa help yousa update!*

</div>
