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

    private readonly MediaPlayer mediaPlayer;
    private CancellationTokenSource? notificationSource;
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

        this.WhenAnyValue(cvm => cvm.loginPage.LoggedIn).Where(loggedIn => loggedIn).Subscribe(_ => GoToContactList());
        this.WhenAnyValue(cvm => cvm.contactListPage.LoggedIn).Where(loggedIn => !loggedIn).Subscribe(_ => GoToLoginPage());
        this.WhenAnyValue(cvm => cvm.conversationPage.LoggedIn).Where(loggedIn => !loggedIn).Subscribe(_ => GoToLoginPage());

        this.WhenAnyValue(cvm => cvm.contactListPage.Chatting).Where(chatting => chatting).Subscribe(_ => GoToConversationPage());
        this.WhenAnyValue(cvm => cvm.contactListPage.Chatting).Where(chatting => !chatting).Subscribe(_ => ReturnToContactList());
        this.WhenAnyValue(cvm => cvm.conversationPage.Chatting).Where(chatting => !chatting).Subscribe(_ => ReturnToContactList());

        contactListPage.NewMessage += ConversationPage_NewMessage;

        LibVLC libVLC = new LibVLC();

        mediaPlayer = new MediaPlayer(libVLC);
        using (Media media = new Media(libVLC, new StreamMediaInput(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/type.wav")))))
            mediaPlayer.Media = media;
    }

    private void GoToContactList()
    {
        contactListPage.NotificationServer = loginPage.NotificationServer;
        contactListPage.Database = loginPage.Database;
        contactListPage.PersonalMessage = contactListPage.NotificationServer.ContactList.Profile.PersonalMessage;
        contactListPage.LoggedIn = true;

        contactListPage.ListData.GetProperties();
        contactListPage.NotificationServer.Disconnected += contactListPage.NotificationServer_Disconnected;
        contactListPage.NotificationServer.SwitchboardChanged += contactListPage.NotificationServer_SwitchboardChanged;

        CurrentPage = contactListPage;
    }

    private void GoToLoginPage()
    {
        loginPage.LoggedIn = false;

        loginPage.GetUsers();
        CurrentPage = loginPage;
    }

    private void GoToConversationPage()
    {
        if (contactListPage.CurrentConversation == null)
            return;

        conversationPage.Conversation = contactListPage.CurrentConversation;
        conversationPage.Database = contactListPage.Database;

        conversationPage.Chatting = true;
        CurrentPage = conversationPage;
    }

    private void ReturnToContactList()
    {
        if (conversationPage.Conversation == null)
            return;

        contactListPage.Chatting = false;
        conversationPage.Chatting = false;
        CurrentPage = contactListPage;
    }

    private async void ConversationPage_NewMessage(object? sender, Models.NewMessageEventArgs e)
    {
        if (NotificationPage != null)
            return;

        mediaPlayer.Stop();
        mediaPlayer.Play();

        if (conversationPage.Conversation?.Contact.Email == e.Sender?.Email && conversationPage.Chatting)
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
