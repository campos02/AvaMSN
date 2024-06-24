using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using AvaMSN.Models;
using AvaMSN.ViewModels;
using ManagedBass;
using ReactiveUI;
using DesktopNotifications;
using NotificationEventArgs = AvaMSN.Models.NotificationEventArgs;

namespace AvaMSN.Utils.Notifications;

/// <summary>
/// Contains functions to show notifications and display errors.
/// </summary>
public class NotificationHandler : ReactiveObject
{
    private ErrorViewModel? errorPage;

    public ErrorViewModel? ErrorPage
    {
        get => errorPage;
        private set => this.RaiseAndSetIfChanged(ref errorPage, value);
    }

    public event EventHandler<ContactEventArgs>? ReplyTapped;
    public event EventHandler<NotificationEventArgs>? NewNotification;
    public event EventHandler? ApplicationExit;

    private CancellationTokenSource delaySource = new CancellationTokenSource();
    private readonly int audioStream;

    public NotificationHandler()
    {
        if (Bass.Init())
        {
            using MemoryStream stream = new();
            AssetLoader.Open(new Uri("avares://AvaMSN.Shared/Assets/type.wav")).CopyTo(stream);
            byte[] audio = stream.ToArray();
            audioStream = Bass.CreateStream(audio, 0, audio.LongLength, BassFlags.Default);
        }
    }

    /// <summary>
    /// Invokes a notification event. A delay task is used as an event argument for syncing (or cancelling) display across all windows.
    /// </summary>
    /// <param name="contact">Message sender.</param>
    /// <param name="message">The message itself.</param>
    /// <returns></returns>
    public async Task InvokeNotification(Contact contact, Message message)
    {
        await delaySource.CancelAsync();
        delaySource = new CancellationTokenSource();

        NewNotification?.Invoke(this, new NotificationEventArgs
        {
            Contact = contact,
            Message = message,
            DelayTask = Task.Delay(10000, delaySource.Token)
        });
    }

    /// <summary>
    /// Shows a native notification.
    /// </summary>
    /// <param name="message">Message to show.</param>
    public static void ShowNativeNotification(Message message)
    {
        if (OperatingSystem.IsMacOSVersionAtLeast(10, 14))
            MacOsNotifications.showNotification($"{message.SenderDisplayName} says:", message.Text);
        else
        {
            var notification = new Notification
            {
                Title = $"{message.SenderDisplayName} says:",
                Body = message.Text
            };
            
            _ = App.NotificationManager?.ShowNotification(notification);
        }
    }

    /// <summary>
    /// Invokes a reply button event.
    /// </summary>
    /// <param name="contact">Message sender.</param>
    /// <returns></returns>
    public void InvokeReplyTapped(Contact? contact)
    {
        ReplyTapped?.Invoke(this, new ContactEventArgs
        {
            Contact = contact
        });
    }

    /// <summary>
    /// Invokes the application exit event.
    /// </summary>
    public void InvokeExit()
    {
        delaySource?.Dispose();
        ApplicationExit?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Plays notification sound.
    /// </summary>
    public void PlaySound()
    {
        Bass.ChannelPlay(audioStream);
    }

    /// <summary>
    /// Calls Bass' free stream functions.
    /// </summary>
    public void FreeStream()
    {
        Bass.StreamFree(audioStream);
        Bass.Free();
    }

    /// <summary>
    /// Displays error page with provided text.
    /// </summary>
    /// <param name="error">Error text to be displayed.</param>
    public void ShowError(string error)
    {
        ErrorPage = new ErrorViewModel()
        {
            Error = error
        };

        ErrorPage.CloseTapped += ErrorPage_CloseTapped;
    }

    /// <summary>
    /// Removes error from screen.
    /// </summary>
    private void ErrorPage_CloseTapped(object? sender, EventArgs e)
    {
        ErrorPage = null;
    }
}
