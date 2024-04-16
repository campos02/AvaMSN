﻿using AvaMSN.Models;
using AvaMSN.MSNP.Messages;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using AvaMSN.Utils;

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

        TextPlain textMessage = new TextPlain
        {
            Bold = Bold,
            Italic = Italic,
            Strikethrough = Strikethrough,
            Underline = Underline,
            Decorations = Decorations,
            Content = Message
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