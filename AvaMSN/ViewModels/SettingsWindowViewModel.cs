using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AvaMSN.Utils;

namespace AvaMSN.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public string Server { get; set; } = string.Empty;
    public bool SaveMessages { get; set; }
    public bool MinimizeToTray { get; set; }

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
        SettingsManager.SaveToFile();

        ResultText = "Saved successfully!";
        await Task.Delay(2000);
        ResultText = string.Empty;
    }
}
