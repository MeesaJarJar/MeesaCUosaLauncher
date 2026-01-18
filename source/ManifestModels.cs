using System.Xml;

namespace JarJarLauncher;

/// <summary>
/// Represents a file entry in the manifest with its hash for verification.
/// </summary>
public sealed class ManifestFile
{
    public string Filename { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;

    public static ManifestFile FromXml(XmlNode node)
    {
        return new ManifestFile
        {
            Filename = node.Attributes?["filename"]?.Value ?? string.Empty,
            Hash = node.Attributes?["hash"]?.Value ?? string.Empty
        };
    }

    public override string ToString() => $"{Filename} ({Hash})";
}

/// <summary>
/// Represents a release version in the manifest.
/// </summary>
public sealed class ManifestRelease
{
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ManifestFile> Files { get; set; } = new();

    public static ManifestRelease FromXml(XmlNode node)
    {
        var release = new ManifestRelease
        {
            Version = node.Attributes?["version"]?.Value ?? string.Empty,
            Name = node.Attributes?["name"]?.Value ?? string.Empty
        };

        var filesNode = node.SelectSingleNode("files");
        if (filesNode != null)
        {
            foreach (XmlNode fileNode in filesNode.SelectNodes("file")!)
            {
                release.Files.Add(ManifestFile.FromXml(fileNode));
            }
        }

        return release;
    }
}

/// <summary>
/// Parses and holds manifest data from the server.
/// </summary>
public sealed class Manifest
{
    public List<ManifestRelease> Releases { get; set; } = new();

    public ManifestRelease? LatestRelease => Releases.FirstOrDefault();

    public static Manifest Parse(string xml)
    {
        var manifest = new Manifest();
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var releaseNodes = doc.SelectNodes("/releases/release");
        if (releaseNodes != null)
        {
            foreach (XmlNode node in releaseNodes)
            {
                manifest.Releases.Add(ManifestRelease.FromXml(node));
            }
        }

        return manifest;
    }
}
