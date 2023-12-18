using AvaMSN.Models;
using AvaMSN.MSNP.Messages;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace AvaMSN.ViewModels;

public class ConversationWindowViewModel : ViewModelBase
{
    private Conversation? conversation;

    public Conversation? Conversation
    {
        get => conversation;
        set => this.RaiseAndSetIfChanged(ref conversation, value);
    }

    private string message = string.Empty;

    public string Message
    {
        get => message;
        set => this.RaiseAndSetIfChanged(ref message, value);
    }

    private NotificationViewModel? notificationPage;

    public NotificationViewModel? NotificationPage
    {
        get => notificationPage;
        private set => this.RaiseAndSetIfChanged(ref notificationPage, value);
    }

    private bool bold;
    private bool strikethrough;
    private bool underline;

    public bool Bold
    {
        get => bold;
        set => this.RaiseAndSetIfChanged(ref bold, value);
    }

    public bool Strikethrough
    {
        get => strikethrough;
        set => this.RaiseAndSetIfChanged(ref strikethrough, value);
    }

    public bool Underline
    {
        get => underline;
        set => this.RaiseAndSetIfChanged(ref underline, value);
    }

    public ReactiveCommand<Unit, Unit> SendCommand { get; }
    public ReactiveCommand<Unit, Unit> NudgeCommand { get; }
    public ReactiveCommand<Unit, Unit> TypingUserCommand { get; }

    public ReactiveCommand<Unit, Unit> CompleteHistoryCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteHistoryCommand { get; }

    public Database? Database { get; set; }

    public ConversationWindowViewModel()
    {
        SendCommand = ReactiveCommand.CreateFromTask(SendMessage);
        NudgeCommand = ReactiveCommand.CreateFromTask(SendNudge);
        TypingUserCommand = ReactiveCommand.CreateFromTask(SendTypingUser);

        CompleteHistoryCommand = ReactiveCommand.Create(GetCompleteHistory);
        DeleteHistoryCommand = ReactiveCommand.Create(DeleteHistory);

        if (NotificationManager != null)
            NotificationManager.Notification += NotificationManager_Notification;
    }

    /// <summary>
    /// Shows notification on screen for the amount of time determined by the event.
    /// </summary>
    private async void NotificationManager_Notification(object? sender, NotificationEventArgs e)
    {
        if (e.Contact == Conversation?.Contact)
            return;

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
            NotificationManager?.InvokeReplyTapped(notification.Sender);

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

    private async Task SendMessage()
    {
        if (Conversation == null)
            return;

        TextPlain message = new TextPlain()
        {
            Bold = Bold,
            Strikethrough = Strikethrough,
            Underline = Underline,
            Content = Message
        };

        await Conversation.SendTextMessage(message);
        Message = string.Empty;
    }

    private async Task SendTypingUser()
    {
        if (Conversation == null)
            return;

        await Conversation.SendTypingUser();
    }

    private async Task SendNudge()
    {
        if (Conversation == null)
            return;

        await Conversation.SendNudge();
    }

    /// <summary>
    /// Gets the full messaging history.
    /// </summary>
    private void GetCompleteHistory()
    {
        Conversation?.GetHistory();
    }

    /// <summary>
    /// Deletes all messages from message history.
    /// </summary>
    private void DeleteHistory()
    {
        Conversation?.DeleteHistory();
    }
}