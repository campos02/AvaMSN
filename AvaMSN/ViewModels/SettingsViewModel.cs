using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace AvaMSN.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    public SettingsManager SettingsManager { get; set; } = new SettingsManager();
    public string Server { get; set; } = string.Empty;

    private string successText = string.Empty;

    public string SuccessText
    {
        get => successText;
        set => this.RaiseAndSetIfChanged(ref successText, value);
    }

    public event EventHandler? BackButtonPressed;

    public SettingsViewModel()
    {
        SaveCommand = ReactiveCommand.CreateFromTask(Save);
        BackCommand = ReactiveCommand.Create(Back);
        Server = SettingsManager.Settings.Server;
    }

    /// <summary>
    /// Convert input values, assign them and call save to settings file function.
    /// If successful, display success message for 2 seconds.
    /// </summary>
    private async Task Save()
    {
        try
        {
            if (Server != string.Empty)
                SettingsManager.Settings.Server = Server;

            SettingsManager.SaveToFile();
        }
        catch (Exception) { return; }

        SuccessText = "Saved successfully! Restart the client to apply new settings";
        await Task.Delay(2000);
        SuccessText = string.Empty;
    }

    private void Back()
    {
        BackButtonPressed?.Invoke(this, EventArgs.Empty);
    }
}
