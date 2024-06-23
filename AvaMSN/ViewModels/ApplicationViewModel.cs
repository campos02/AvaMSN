using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System.Reactive;

namespace AvaMSN.ViewModels;

public class ApplicationViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> OpenCommand { get; } = ReactiveCommand.Create(OpenWindows);
    public ReactiveCommand<Unit, Unit> MinimizeCommand { get; } = ReactiveCommand.Create(MinimizeToTray);
    public ReactiveCommand<Unit, Unit> ExitCommand { get; } = ReactiveCommand.Create(Exit);

    /// <summary>
    /// Opens all windows and brings them to focus.
    /// </summary>
    public static void OpenWindows()
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
    public static void MinimizeToTray()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            for (int i = 0; i < desktop.Windows.Count; i++)
            {
                desktop.Windows[i].Hide();
            }
        }
    }

    public static void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }
}
