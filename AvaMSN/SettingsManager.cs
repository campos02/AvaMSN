using System;
using System.IO;
using System.Text.Json;
using AvaMSN.Models;

namespace AvaMSN;

/// <summary>
/// Provides access to client settings and functions to save them to the settings file.
/// </summary>
public class SettingsManager
{
    public static string FileName => "settings.json";
    public static string FileDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AvaMSN");
    public static string FilePath => Path.Combine(FileDirectory, FileName);

    /// <summary>
    /// Client settings. When modified, they change across all manager objects.
    /// </summary>
    public static Settings Settings { get; set; } = new Settings()
    {
        // Default settings
        Server = "crosstalksrv.hiden.pw",
        SaveMessagingHistory = true
    };

    public SettingsManager()
    {
        ReadFile();
    }

    /// <summary>
    /// Create file and directory if they don't exist and save serialized settings.
    /// </summary>
    public void SaveToFile()
    {
        if (!Directory.Exists(FileDirectory))
            Directory.CreateDirectory(FileDirectory);

        var options = new JsonSerializerOptions() { WriteIndented = true };
        File.WriteAllText(FilePath, JsonSerializer.Serialize(Settings, options));
    }

    /// <summary>
    /// Read file, if it exists, and assign deserialized settings.
    /// </summary>
    public void ReadFile()
    {
        if (!File.Exists(FilePath))
            return;

        var json = File.ReadAllText(FilePath);
        Settings = JsonSerializer.Deserialize<Settings>(json) ?? Settings;
    }
}
