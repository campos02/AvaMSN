using System.IO;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AvaMSN.Utils;
using AvaMSN.Models;

namespace AvaMSN.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public static Settings Settings { get; set; } = new Settings();
    public bool SaveMessages { get; set; }
    public bool MinimizeToTray { get; set; }
    public bool SaveConnectionLog { get; set; }
    public string LogPath { get; init; } = $"Log location: {Path.Combine(SettingsManager.FileDirectory, "connection.log")}";

    private string resultText = string.Empty;

    public string ResultText
    {
        get => resultText;
        set => this.RaiseAndSetIfChanged(ref resultText, value);
    }

    public SettingsWindowViewModel()
    {
        SaveCommand = ReactiveCommand.CreateFromTask(Save);
        Settings.MainServer = SettingsManager.Settings.MainServer;
        Settings.RstUrl = SettingsManager.Settings.RstUrl;
        Settings.SharingServiceUrl = SettingsManager.Settings.SharingServiceUrl;
        Settings.AddressBookUrl = SettingsManager.Settings.AddressBookUrl;
        SaveMessages = SettingsManager.Settings.SaveMessagingHistory;
        MinimizeToTray = SettingsManager.Settings.MinimizeToTray;
        SaveConnectionLog = SettingsManager.Settings.SaveConnectionLog;
    }

    /// <summary>
    /// Assigns input values to model and calls save to settings file function.
    /// </summary>
    private async Task Save()
    {
        if (Settings.MainServer != string.Empty)
            SettingsManager.Settings.MainServer = Settings.MainServer;

        if (Settings.RstUrl != string.Empty)
            SettingsManager.Settings.RstUrl = Settings.RstUrl;

        if (Settings.SharingServiceUrl != string.Empty)
            SettingsManager.Settings.SharingServiceUrl = Settings.SharingServiceUrl;

        if (Settings.AddressBookUrl != string.Empty)
            SettingsManager.Settings.AddressBookUrl = Settings.AddressBookUrl;

        SettingsManager.Settings.SaveMessagingHistory = SaveMessages;
        SettingsManager.Settings.MinimizeToTray = MinimizeToTray;
        SettingsManager.Settings.SaveConnectionLog = SaveConnectionLog;
        SettingsManager.SaveToFile();

        ResultText = "Saved successfully!";
        await Task.Delay(2000);
        ResultText = string.Empty;
    }
}
