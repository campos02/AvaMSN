using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaMSN.Models;
using AvaMSN.ViewModels;
using AvaMSN.Views;
using ReactiveUI;

namespace AvaMSN;

public partial class App : Application
{
    private readonly ExceptionHandler handler = new ExceptionHandler();

    public App()
    {
        DataContext = new ApplicationViewModel();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        RxApp.DefaultExceptionHandler = handler;
        ViewModelBase.NotificationManager = handler.NotificationManager;
        Conversation.NotificationManager = handler.NotificationManager;

        SettingsManager.ReadFile();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit += Desktop_Exit;

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            desktop.MainWindow.Closing += MainWindow_Closing;
            desktop.MainWindow.Closed += MainWindow_Closed;
        }

        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (SettingsManager.Settings.MinimizeToTray)
        {
            if (sender is MainWindow mainWindow)
                mainWindow.Hide();

            e.Cancel = true;
        }
    }

    private void MainWindow_Closed(object? sender, System.EventArgs e)
    {
        if (!SettingsManager.Settings.MinimizeToTray)
        {
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }

    private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        handler.NotificationManager.FreeStream();
        handler.NotificationManager.InvokeExit();
    }
}
