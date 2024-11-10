using AvaMSN.Models;
using AvaMSN.MSNP.Messages;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media;

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
    private bool italic;
    private bool underline;
    private bool strikethrough;
    private string decorations = string.Empty;
    private string color = string.Empty;
    private Color pickerColor = Colors.Black;

    public bool Bold
    {
        get => bold;
        set => this.RaiseAndSetIfChanged(ref bold, value);
    }

    public bool Italic
    {
        get => italic;
        set => this.RaiseAndSetIfChanged(ref italic, value);
    }

    public bool Underline
    {
        get => underline;
        set
        {
            this.RaiseAndSetIfChanged(ref underline, value);
            SetDecorations();
        }
    }
    
    public bool Strikethrough
    {
        get => strikethrough;
        set
        {
            this.RaiseAndSetIfChanged(ref strikethrough, value);
            SetDecorations();
        }
    }

    public string Decorations
    {
        get => decorations;
        set => this.RaiseAndSetIfChanged(ref decorations, value);
    }

    public string MessageColor
    {
        get => color;
        set => this.RaiseAndSetIfChanged(ref color, value);
    }

    public Color PickerColor
    {
        get => pickerColor;
        set
        {
            pickerColor = value;
            MessageColor = $"#{pickerColor.R:x2}{pickerColor.G:x2}{pickerColor.B:x2}";
        }
    }

    public ReactiveCommand<Unit, Unit> SendCommand { get; }
    public ReactiveCommand<Unit, Unit> NudgeCommand { get; }
    public ReactiveCommand<Unit, Unit> TypingUserCommand { get; }
    public ReactiveCommand<Unit, Unit> CompleteHistoryCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteHistoryCommand { get; }

    public ConversationWindowViewModel()
    {
        SendCommand = ReactiveCommand.CreateFromTask(SendMessage);
        NudgeCommand = ReactiveCommand.CreateFromTask(SendNudge);
        TypingUserCommand = ReactiveCommand.CreateFromTask(SendTypingUser);
        CompleteHistoryCommand = ReactiveCommand.Create(GetCompleteHistory);
        DeleteHistoryCommand = ReactiveCommand.Create(DeleteHistory);

        if (NotificationHandler != null)
            NotificationHandler.NewNotification += NotificationHandler_Notification;
    }

    /// <summary>
    /// Shows notification on screen for the amount of time determined by the event.
    /// </summary>
    private async void NotificationHandler_Notification(object? sender, NotificationEventArgs e)
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

    private async Task SendMessage()
    {
        if (Conversation == null)
            return;

        TextPlain textMessage = new TextPlain
        {
            Bold = Bold,
            Italic = Italic,
            Strikethrough = Strikethrough,
            Underline = Underline,
            Decorations = Decorations,
            Color = MessageColor,
            Text = Message
        };

        Message = string.Empty;
        await Conversation.SendTextMessage(textMessage);
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

    private void SetDecorations()
    {
        string newDecorations = string.Empty;
        if (Underline)
            newDecorations += "Underline";
        if (Strikethrough)
            newDecorations += " Strikethrough";

        Decorations = newDecorations;
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