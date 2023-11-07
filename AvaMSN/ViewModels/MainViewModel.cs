using ReactiveUI;
using System.Reactive.Linq;
using System;
using System.Threading.Tasks;
using System.Threading;
using LibVLCSharp.Shared;
using Avalonia.Platform;

namespace AvaMSN.ViewModels;

public class MainViewModel : ViewModelBase
{
    private LoginViewModel loginPage = new();
    private ContactListViewModel contactListPage = new();
    private ConversationViewModel conversationPage = new();
    private SettingsViewModel settingsPage = new();

    private readonly MediaPlayer mediaPlayer;
    private CancellationTokenSource? notificationSource;
    private ViewModelBase? previousPage;
    private ViewModelBase currentPage;

    public ViewModelBase CurrentPage
    {
        get => currentPage;
        set => this.RaiseAndSetIfChanged(ref currentPage, value);
    }

    private NotificationViewModel? notificationPage;

    public NotificationViewModel? NotificationPage
    {
        get => notificationPage;
        set => this.RaiseAndSetIfChanged(ref notificationPage, value);
    }

    public MainViewModel()
    {
        currentPage = loginPage;

        loginPage.LoggedIn += LoginPage_LoggedIn;
        contactListPage.Disconnected += ContactListPage_Disconnected;

        contactListPage.ChatStarted += ContactListPage_ChatStarted;
        contactListPage.NewMessage += ConversationPage_NewMessage;

        contactListPage.OptionsButtonPressed += Pages_OptionsButtonPressed;
        loginPage.OptionsButtonPressed += Pages_OptionsButtonPressed;
        settingsPage.BackButtonPressed += SettingsPage_BackButtonPressed;
        conversationPage.BackButtonPressed += ConversationPage_BackButtonPressed;

        LibVLC libVLC = new LibVLC();

        mediaPlayer = new MediaPlayer(libVLC);
        using (Media media = new Media(libVLC, new StreamMediaInput(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/type.wav")))))
            mediaPlayer.Media = media;
    }

    private void ConversationPage_BackButtonPressed(object? sender, EventArgs e)
    {
        CurrentPage = contactListPage;
    }

    private void SettingsPage_BackButtonPressed(object? sender, EventArgs e)
    {
        CurrentPage = previousPage ?? CurrentPage;
        previousPage = null;
    }

    private void ContactListPage_ChatStarted(object? sender, EventArgs e)
    {
        if (contactListPage.CurrentConversation == null)
            return;

        conversationPage.Conversation = contactListPage.CurrentConversation;
        conversationPage.Database = contactListPage.Database;
        CurrentPage = conversationPage;
    }

    private void LoginPage_LoggedIn(object? sender, EventArgs e)
    {
        contactListPage.NotificationServer = loginPage.NotificationServer;
        contactListPage.Database = loginPage.Database;
        contactListPage.PersonalMessage = contactListPage.NotificationServer.ContactList.Profile.PersonalMessage;

        contactListPage.ListData.GetProperties();
        contactListPage.NotificationServer.Disconnected += contactListPage.NotificationServer_Disconnected;
        contactListPage.NotificationServer.SwitchboardChanged += contactListPage.NotificationServer_SwitchboardChanged;

        CurrentPage = contactListPage;
    }

    private void ContactListPage_Disconnected(object? sender, EventArgs e)
    {
        loginPage.GetUsers();
        CurrentPage = loginPage;
    }

    private void Pages_OptionsButtonPressed(object? sender, EventArgs e)
    {
        previousPage = CurrentPage;
        CurrentPage = settingsPage;
    }

    private async void ConversationPage_NewMessage(object? sender, Models.NewMessageEventArgs e)
    {
        if (NotificationPage != null)
            return;

        mediaPlayer.Stop();
        mediaPlayer.Play();

        if (conversationPage.Conversation?.Contact.Email == e.Sender?.Email && CurrentPage is ConversationViewModel)
            return;

        NotificationPage = new NotificationViewModel()
        {
            Sender = e.Sender,
            Message = e.Message
        };

        NotificationPage.ReplyTapped += NotificationPage_ReplyTapped;
        NotificationPage.DismissTapped += NotificationPage_DismissTapped;

        notificationSource = new CancellationTokenSource();
        try
        {
            await Task.Delay(5000, notificationSource.Token);
        }
        catch (OperationCanceledException) { }

        CloseNotification();
    }

    private async void NotificationPage_ReplyTapped(object? sender, EventArgs e)
    {
        contactListPage.SelectedContact = NotificationPage?.Sender;
        await contactListPage.Chat();

        notificationSource?.Cancel();
    }

    private void NotificationPage_DismissTapped(object? sender, EventArgs e)
    {
        notificationSource?.Cancel();
    }

    private void CloseNotification()
    {
        if (NotificationPage == null)
            return;

        NotificationPage!.ReplyTapped -= NotificationPage_ReplyTapped;
        NotificationPage.DismissTapped -= NotificationPage_DismissTapped;
        NotificationPage = null;
    }
}
