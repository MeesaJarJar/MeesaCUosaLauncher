using System.Text.Json.Serialization;

namespace JarJarLauncher;

/// <summary>
/// Represents a launch argument for the client.
/// </summary>
public sealed class LaunchArgument
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("is_quoted")]
    public bool IsQuoted { get; set; } = false;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// UI customization settings for the launcher.
/// </summary>
public sealed class UISettings
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "JarJar Launcher";

    [JsonPropertyName("window_width")]
    public int WindowWidth { get; set; } = 720;

    [JsonPropertyName("window_height")]
    public int WindowHeight { get; set; } = 580;

    [JsonPropertyName("theme")]
    public UITheme Theme { get; set; } = new();

    [JsonPropertyName("background_image_opacity")]
    public double BackgroundImageOpacity { get; set; } = 0.3;

    [JsonPropertyName("show_uo_directory_field")]
    public bool ShowUODirectoryField { get; set; } = true;

    [JsonPropertyName("show_version_label")]
    public bool ShowVersionLabel { get; set; } = true;

    [JsonPropertyName("play_button_text")]
    public string PlayButtonText { get; set; } = "PLAY";

    [JsonPropertyName("check_button_text")]
    public string CheckButtonText { get; set; } = "Check for Updates";

    [JsonPropertyName("update_button_text")]
    public string UpdateButtonText { get; set; } = "Update Now";
}

/// <summary>
/// Color theme for the launcher UI.
/// </summary>
public sealed class UITheme
{
    [JsonPropertyName("background_color")]
    public string BackgroundColor { get; set; } = "#1E1E23";

    [JsonPropertyName("title_color")]
    public string TitleColor { get; set; } = "#64C8FF";

    [JsonPropertyName("text_color")]
    public string TextColor { get; set; } = "#FFFFFF";

    [JsonPropertyName("button_background")]
    public string ButtonBackground { get; set; } = "#3C3C46";

    [JsonPropertyName("button_text_color")]
    public string ButtonTextColor { get; set; } = "#FFFFFF";

    [JsonPropertyName("play_button_background")]
    public string PlayButtonBackground { get; set; } = "#3278C8";

    [JsonPropertyName("update_button_background")]
    public string UpdateButtonBackground { get; set; } = "#468246";

    [JsonPropertyName("update_button_highlight")]
    public string UpdateButtonHighlight { get; set; } = "#C89632";

    [JsonPropertyName("log_background")]
    public string LogBackground { get; set; } = "#141419";

    [JsonPropertyName("log_text_color")]
    public string LogTextColor { get; set; } = "#90EE90";

    [JsonPropertyName("textbox_background")]
    public string TextBoxBackground { get; set; } = "#323237";

    [JsonPropertyName("textbox_valid_background")]
    public string TextBoxValidBackground { get; set; } = "#284628";

    [JsonPropertyName("textbox_invalid_background")]
    public string TextBoxInvalidBackground { get; set; } = "#462828";

    /// <summary>
    /// Converts hex color string to Color object.
    /// </summary>
    public static Color ParseColor(string hexColor, Color defaultColor)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hexColor))
                return defaultColor;

            hexColor = hexColor.TrimStart('#');
            
            if (hexColor.Length == 6)
            {
                int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                return Color.FromArgb(r, g, b);
            }
        }
        catch
        {
            // Return default on parse error
        }

        return defaultColor;
    }
}
