using AvaMSN.Models;
using ReactiveUI;
using System;

namespace AvaMSN.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private NotificationViewModel? notificationPage;
    private LoginViewModel loginPage = new LoginViewModel();
    private ContactListViewModel contactListPage = new ContactListViewModel();
    private ViewModelBase currentPage;

    public ViewModelBase CurrentPage
    {
        get => currentPage;
        set => this.RaiseAndSetIfChanged(ref currentPage, value);
    }

    public NotificationViewModel? NotificationPage
    {
        get => notificationPage;
        private set => this.RaiseAndSetIfChanged(ref notificationPage, value);
    }

    public MainWindowViewModel()
    {
        currentPage = loginPage;

        loginPage.LoggedIn += LoginPage_LoggedIn;
        contactListPage.Disconnected += ContactListPage_Disconnected;

        if (NotificationHandler != null)
            NotificationHandler.NewNotification += NotificationHandler_Notification;
    }

    /// <summary>
    /// Shows notification on screen for the amount of time determined by the event.
    /// </summary>
    private async void NotificationHandler_Notification(object? sender, NotificationEventArgs e)
    {
        NotificationPage = new NotificationViewModel()
        {
            Sender = e.Contact,
            Message = e.Message
        };

        NotificationPage.ReplyTapped += NotificationPage_ReplyTapped;
        NotificationPage.DismissTapped += NotificationPage_DismissTapped;

        if (e.DelayTask != null)
        {
            try
            {
                await e.DelayTask;
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        CloseNotification();
    }

    private void NotificationPage_ReplyTapped(object? sender, EventArgs e)
    {
        if (sender is NotificationViewModel notification)
            NotificationHandler?.InvokeReplyTapped(notification.Sender);

        CloseNotification();
    }

    private void NotificationPage_DismissTapped(object? sender, EventArgs e)
    {
        CloseNotification();
    }

    /// <summary>
    /// Removes notification from the screen.
    /// </summary>
    private void CloseNotification()
    {
        if (NotificationPage == null)
            return;

        NotificationPage.ReplyTapped -= NotificationPage_ReplyTapped;
        NotificationPage.DismissTapped -= NotificationPage_DismissTapped;
        NotificationPage = null;
    }

    private void LoginPage_LoggedIn(object? sender, LoggedInEventArgs e)
    {
        contactListPage.UserProfile = e.UserProfile;
        contactListPage.ContactActions = e.ContactActions;
        contactListPage.Database = loginPage.Database;
        contactListPage.PersonalMessage = contactListPage.UserProfile?.User?.PersonalMessage ?? string.Empty;
        contactListPage.User.DisplayPicture = loginPage.User.DisplayPicture;

        contactListPage.ListData.GetData();
        contactListPage.SubscribeToEvents();

        CurrentPage = contactListPage;
    }

    private async void ContactListPage_Disconnected(object? sender, DisconnectedEventArgs e)
    {
        loginPage.GetUsers();
        CurrentPage = loginPage;
        if (e.RedirectedByTheServer)
        {
            loginPage.ComingFromAPreviousRedirection = true;
            loginPage.NewServer = e.NewServerHost;
            loginPage.NewPort = e.NewServerPort;

            loginPage.SignInButtonEnabled = false;
            await loginPage.SignIn();
            loginPage.SignInButtonEnabled = true;
        }
    }
}
