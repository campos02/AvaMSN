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
    private NotificationViewModel? notificationPage;
    private ErrorViewModel? errorPage;
    private readonly MediaPlayer mediaPlayer;
    private CancellationTokenSource? cancellationSource;

    public NotificationViewModel? NotificationPage
    {
        get => notificationPage;
        private set => this.RaiseAndSetIfChanged(ref notificationPage, value);
    }

    public ErrorViewModel? ErrorPage
    {
        get => errorPage;
        private set => this.RaiseAndSetIfChanged(ref errorPage, value);
    }

    public event EventHandler? ReplyTapped;

    public NotificationManager()
    {
        LibVLC libVLC = new LibVLC();
        mediaPlayer = new MediaPlayer(libVLC);

        // Load notification sound
        using Media media = new Media(libVLC, new StreamMediaInput(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/type.wav"))));
        mediaPlayer.Media = media;
    }

    /// <summary>
    /// Shows a new message notification for five seconds.
    /// </summary>
    /// <param name="contact">Message sender.</param>
    /// <param name="message">The message itself.</param>
    /// <returns></returns>
    public async Task ShowNotification(Contact? contact, Message? message)
    {
        if (NotificationPage != null)
            return;

        PlaySound();

        NotificationPage = new NotificationViewModel()
        {
            Sender = contact,
            Message = message
        };

        NotificationPage.ReplyTapped += NotificationPage_ReplyTapped;
        NotificationPage.DismissTapped += NotificationPage_DismissTapped;

        // Stop current delay and create new cancellation source for new delay
        cancellationSource?.Cancel();
        cancellationSource = new CancellationTokenSource();
        try
        {
            await Task.Delay(5000, cancellationSource.Token);
        }
        catch (OperationCanceledException) { }

        CloseNotification();
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

    private void NotificationPage_ReplyTapped(object? sender, EventArgs e)
    {
        ReplyTapped?.Invoke(this, new EventArgs());
        cancellationSource?.Cancel();
    }

    private void NotificationPage_DismissTapped(object? sender, EventArgs e)
    {
        cancellationSource?.Cancel();
    }

    /// <summary>
    /// Removes notification from the screen.
    /// </summary>
    private void CloseNotification()
    {
        if (NotificationPage == null)
            return;

        NotificationPage!.ReplyTapped -= NotificationPage_ReplyTapped;
        NotificationPage.DismissTapped -= NotificationPage_DismissTapped;
        NotificationPage = null;
    }
}
