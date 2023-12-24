using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using System;

namespace AvaMSN.Models;

/// <summary>
/// Represents contact data.
/// </summary>
public class Contact : ReactiveObject
{
    private string displayName = string.Empty;
    private string email = string.Empty;
    private string personalMessage = string.Empty;
    private string presence = string.Empty;
    private string color = string.Empty;
    private bool newMessages;
    private bool blocked;
    private Bitmap displayPicture;

    public string DisplayName
    {
        get => displayName;
        set => this.RaiseAndSetIfChanged(ref displayName, value);
    }

    public string Email
    {
        get => email;
        set => this.RaiseAndSetIfChanged(ref email, value);
    }

    public string PersonalMessage
    {
        get => personalMessage;
        set => this.RaiseAndSetIfChanged(ref personalMessage, value);
    }

    /// <summary>
    /// Stores a full name presence.
    /// </summary>
    public string Presence
    {
        get => presence;
        set => this.RaiseAndSetIfChanged(ref presence, value);
    }

    public string PresenceColor
    {
        get => color;
        set => this.RaiseAndSetIfChanged(ref color, value);
    }

    public bool NewMessages
    {
        get => newMessages;
        set => this.RaiseAndSetIfChanged(ref newMessages, value);
    }

    public bool Blocked
    {
        get => blocked;
        set => this.RaiseAndSetIfChanged(ref blocked, value);
    }

    public Bitmap DisplayPicture
    {
        get => displayPicture;
        set => this.RaiseAndSetIfChanged(ref displayPicture, value);
    }

    public string? DisplayPictureHash { get; set; }

    public Contact()
    {
        displayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/default-display-picture.png")));
    }
}
