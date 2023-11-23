using ReactiveUI;
using System;

namespace AvaMSN.ViewModels;

public class MainViewModel : ViewModelBase
{
    private LoginViewModel loginPage = new LoginViewModel();
    private ContactListViewModel contactListPage = new ContactListViewModel();
    private ConversationViewModel conversationPage = new ConversationViewModel();
    private SettingsViewModel settingsPage = new SettingsViewModel();

    private ViewModelBase? previousPage;
    private ViewModelBase currentPage;

    public ViewModelBase CurrentPage
    {
        get => currentPage;
        set => this.RaiseAndSetIfChanged(ref currentPage, value);
    }

    public MainViewModel()
    {
        currentPage = loginPage;

        loginPage.LoggedIn += LoginPage_LoggedIn;
        contactListPage.Disconnected += ContactListPage_Disconnected;
        contactListPage.ChatStarted += ContactListPage_ChatStarted;
        contactListPage.NewMessage += ContactListPage_NewMessage;

        contactListPage.OptionsButtonPressed += Pages_OptionsButtonPressed;
        loginPage.OptionsButtonPressed += Pages_OptionsButtonPressed;
        settingsPage.BackButtonPressed += SettingsPage_BackButtonPressed;
        conversationPage.BackButtonPressed += ConversationPage_BackButtonPressed;
    }

    private async void ContactListPage_NewMessage(object? sender, Models.NewMessageEventArgs e)
    {
        if (contactListPage.CurrentConversation?.Contact.Email == e?.Contact?.Email && CurrentPage is ConversationViewModel)
        {
            NotificationManager?.PlaySound();
        }

        else if (NotificationManager != null)
        {
            e!.Contact!.NewMessages = true;
            await NotificationManager.ShowNotification(e?.Contact, e?.Message);
        }
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
        conversationPage.Conversation = contactListPage?.CurrentConversation;
        conversationPage.Database = contactListPage?.Database;
        CurrentPage = conversationPage;
    }

    private void LoginPage_LoggedIn(object? sender, EventArgs e)
    {
        contactListPage.NotificationServer = loginPage.NotificationServer;
        contactListPage.Database = loginPage.Database;
        contactListPage.PersonalMessage = contactListPage.NotificationServer!.Profile.PersonalMessage;
        contactListPage.Profile.DisplayPicture = loginPage.Profile.DisplayPicture;

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
}
