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
using Serilog;
using System.IO;

namespace AvaMSN;

public class App : Application
{
    private readonly ExceptionHandler handler = new ExceptionHandler();
    public static INotificationManager? NotificationManager { get; private set; }

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
        ViewModelBase.NotificationHandler = handler.NotificationHandler;
        Conversation.NotificationHandler = handler.NotificationHandler;
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

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Conditional(_ => SettingsManager.Settings.SaveConnectionLog,
                    writeTo => writeTo.File(Path.Combine(SettingsManager.FileDirectory, "connection.log")))
                .CreateLogger();
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
        
        _ = NotificationManager?.Initialize();
        if (NotificationManager != null)
            NotificationManager.NotificationActivated += NotificationManager_NotificationActivated;
    }

    private static void NotificationManager_NotificationActivated(object? sender, NotificationActivatedEventArgs e)
    {
        ApplicationViewModel.OpenWindows();
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

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        if (!SettingsManager.Settings.MinimizeToTray)
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
    }

    private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        handler.NotificationHandler.FreeStream();
        handler.NotificationHandler.InvokeExit();
        NotificationManager?.Dispose();
        Log.CloseAndFlush();
    }
}
