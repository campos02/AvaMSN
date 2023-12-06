using Avalonia.Platform;
using AvaMSN.Models;
using AvaMSN.ViewModels;
using LibVLCSharp.Shared;
using ReactiveUI;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AvaMSN;

/// <summary>
/// Contains functions to show notifications and display errors and their respective pages.
/// </summary>
public class NotificationManager : ReactiveObject
{
    private ErrorViewModel? errorPage;
    private readonly MediaPlayer mediaPlayer;

    public ErrorViewModel? ErrorPage
    {
        get => errorPage;
        private set => this.RaiseAndSetIfChanged(ref errorPage, value);
    }

    public event EventHandler<ContactEventArgs>? ReplyTapped;
    public event EventHandler<NotificationEventArgs>? Notification;

    private CancellationTokenSource? delaySource;

    public NotificationManager()
    {
        LibVLC libVLC = new LibVLC();
        mediaPlayer = new MediaPlayer(libVLC);

        // Load notification sound
        using Media media = new Media(libVLC, new StreamMediaInput(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/type.wav"))));
        mediaPlayer.Media = media;
    }

    /// <summary>
    /// Invokes a notification event. A delay task is used as an event argument for syncing (or cancelling) display across all windows.
    /// </summary>
    /// <param name="contact">Message sender.</param>
    /// <param name="message">The message itself.</param>
    /// <returns></returns>
    public void InvokeNotification(Contact? contact, Message? message)
    {
        PlaySound();

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
    /// Plays notification sound.
    /// </summary>
    public void PlaySound()
    {
        mediaPlayer.Stop();
        mediaPlayer.Play();
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
