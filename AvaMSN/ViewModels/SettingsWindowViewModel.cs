using System.IO;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AvaMSN.Utils;

namespace AvaMSN.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public string Server { get; set; }
    public bool SaveMessages { get; set; }
    public bool MinimizeToTray { get; set; }
    public bool SaveConnectionLog { get; set; }
    public static string LogPath => $"Log location: {Path.Combine(SettingsManager.FileDirectory, "connection.log")}";

    private string resultText = string.Empty;

    public string ResultText
    {
        get => resultText;
        set => this.RaiseAndSetIfChanged(ref resultText, value);
    }

    public SettingsWindowViewModel()
    {
        SaveCommand = ReactiveCommand.CreateFromTask(Save);
        Server = SettingsManager.Settings.Server;
        SaveMessages = SettingsManager.Settings.SaveMessagingHistory;
        MinimizeToTray = SettingsManager.Settings.MinimizeToTray;
        SaveConnectionLog = SettingsManager.Settings.SaveConnectionLog;
    }

    /// <summary>
    /// Assigns input values to model and calls save to settings file function.
    /// </summary>
    private async Task Save()
    {
        if (Server != string.Empty)
            SettingsManager.Settings.Server = Server;

        SettingsManager.Settings.SaveMessagingHistory = SaveMessages;
        SettingsManager.Settings.MinimizeToTray = MinimizeToTray;
        SettingsManager.Settings.SaveConnectionLog = SaveConnectionLog;
        SettingsManager.SaveToFile();

        ResultText = "Saved successfully!";
        await Task.Delay(2000);
        ResultText = string.Empty;
    }
}
