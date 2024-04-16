using System;
using System.IO;
using System.Text.Json;
using AvaMSN.Models;

namespace AvaMSN.Utils;

/// <summary>
/// Static class that provides access to client settings as well as functions to save and read from the settings file.
/// </summary>
public static class SettingsManager
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
        SaveMessagingHistory = true,
        MinimizeToTray = true
    };

    /// <summary>
    /// Create file and directory if they don't exist and save serialized settings.
    /// </summary>
    public static void SaveToFile()
    {
        if (!Directory.Exists(FileDirectory))
            Directory.CreateDirectory(FileDirectory);

        var options = new JsonSerializerOptions() { WriteIndented = true };
        File.WriteAllText(FilePath, JsonSerializer.Serialize(Settings, options));
    }

    /// <summary>
    /// Read file, if it exists, and assign deserialized settings.
    /// </summary>
    public static void ReadFile()
    {
        if (!File.Exists(FilePath))
            return;

        var json = File.ReadAllText(FilePath);
        Settings = JsonSerializer.Deserialize<Settings>(json) ?? Settings;
    }
}
