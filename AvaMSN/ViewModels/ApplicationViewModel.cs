using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System.Reactive;

namespace AvaMSN.ViewModels;

public class ApplicationViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> OpenCommand { get; }
    public ReactiveCommand<Unit, Unit> MinimizeCommand { get; }
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }

    public ApplicationViewModel()
    {
        OpenCommand = ReactiveCommand.Create(Open);
        MinimizeCommand = ReactiveCommand.Create(MinimizeToTray);
        ExitCommand = ReactiveCommand.Create(Exit);
    }

    /// <summary>
    /// Opens all windows and brings them to focus.
    /// </summary>
    public void Open()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            for (int i = 0; i < desktop.Windows.Count; i++)
            {
                desktop.Windows[i].Show();
                desktop.Windows[i].Activate();
            }
        }
    }

    /// <summary>
    /// Hides all windows.
    /// </summary>
    public void MinimizeToTray()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            for (int i = 0; i < desktop.Windows.Count; i++)
            {
                desktop.Windows[i].Hide();
            }
        }
    }

    public void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
