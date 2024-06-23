using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaMSN.Models;
using AvaMSN.Utils;
using AvaMSN.Utils.Notifications;
using AvaMSN.ViewModels;
using AvaMSN.Views;
using ReactiveUI;
using DesktopNotifications;
using DesktopNotifications.FreeDesktop;
using DesktopNotifications.Windows;

namespace AvaMSN;

public class App : Application
{
    private readonly ExceptionHandler handler = new ExceptionHandler();
    public static INotificationManager? NotificationManager;

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
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            desktop.Exit += Desktop_Exit;
            desktop.MainWindow.Closing += MainWindow_Closing;
            desktop.MainWindow.Closed += MainWindow_Closed;

            SetupDesktopNotifications();
            if (OperatingSystem.IsMacOSVersionAtLeast(10, 14))
                MacOsNotifications.requestAuthorization();
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
    
    /// <summary>
    /// Setups the <see cref="INotificationManager" /> for the current platform and
    /// binds it to the service locator (<see cref="AvaloniaLocator" />).
    /// Moved to the app class to avoid creating a circular dependency on
    /// Avalonia 11's desktop project template.
    /// </summary>
    /// <returns></returns>
    private static void SetupDesktopNotifications()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var context = WindowsApplicationContext.FromCurrentProcess();
            NotificationManager = new WindowsNotificationManager(context);
        }
        else if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            var context = FreeDesktopApplicationContext.FromCurrentProcess();
            NotificationManager = new FreeDesktopNotificationManager(context);
        }
        else
        {
            //TODO: OSX once implemented/stable
            NotificationManager = null;
        }

        //TODO Any better way of doing this?
        NotificationManager?.Initialize().GetAwaiter().GetResult();
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
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
    }

    private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        handler.NotificationManager.FreeStream();
        handler.NotificationManager.InvokeExit();
        NotificationManager?.Dispose();
    }
}
