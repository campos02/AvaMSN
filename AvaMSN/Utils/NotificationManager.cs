using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using AvaMSN.Models;
using AvaMSN.ViewModels;
using ManagedBass;
using ReactiveUI;

namespace AvaMSN.Utils;

/// <summary>
/// Contains functions to show notifications and display errors.
/// </summary>
public class NotificationManager : ReactiveObject
{
    private ErrorViewModel? errorPage;

    public ErrorViewModel? ErrorPage
    {
        get => errorPage;
        private set => this.RaiseAndSetIfChanged(ref errorPage, value);
    }

    public event EventHandler<ContactEventArgs>? ReplyTapped;
    public event EventHandler<NotificationEventArgs>? Notification;
    public event EventHandler? ApplicationExit;

    private CancellationTokenSource? delaySource;
    private readonly int audioStream;

    public NotificationManager()
    {
        if (Bass.Init())
        {
            using MemoryStream stream = new();
            AssetLoader.Open(new Uri("avares://AvaMSN/Assets/type.wav")).CopyTo(stream);
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
    public void InvokeNotification(Contact? contact, Message? message)
    {
        delaySource?.Cancel();
        delaySource = new CancellationTokenSource();

        Notification?.Invoke(this, new NotificationEventArgs
        {
            Contact = contact,
            Message = message,
            DelayTask = Task.Delay(5000, delaySource.Token)
        });
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
