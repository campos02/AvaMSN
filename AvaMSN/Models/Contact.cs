﻿using ReactiveUI;

namespace AvaMSN.Models;

public class Contact : ReactiveObject
{
    private string displayName = string.Empty;
    private string email = string.Empty;
    private string personalMessage = string.Empty;
    private string presence = string.Empty;
    private string color = string.Empty;

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

    public string Presence
    {
        get => presence;
        set => this.RaiseAndSetIfChanged(ref presence, value);
    }

    public string Color
    {
        get => color;
        set => this.RaiseAndSetIfChanged(ref color, value);
    }
}
