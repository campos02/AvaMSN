using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using AvaMSN.MSNP.Messages;
using AvaMSN.MSNP.Models;
using AvaMSN.MSNP.NotificationServer.Contacts;
using AvaMSN.MSNP.Switchboard;
using AvaMSN.MSNP.Switchboard.Messaging;
using AvaMSN.Utils;
using AvaMSN.Utils.Notifications;
using AvaMSN.ViewModels;
using AvaMSN.Views;
using ReactiveUI;

namespace AvaMSN.Models;

/// <summary>
/// Wrapper for different switchboard sessions and operations with a contact.
/// </summary>
public class Conversation : ReactiveObject
{
    private bool typingUser;
    private ObservableCollection<Message>? messageHistory;
    private ConversationWindow? conversationWindow;

    public User User { get; }
    public Contact Contact { get; }
    private Database? Database { get; }

    public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
    public ObservableCollection<Message>? MessageHistory
    {
        get => messageHistory;
        set => this.RaiseAndSetIfChanged(ref messageHistory, value);
    }

    /// <summary>
    /// Controls whether the "is writing..." message is shown.
    /// </summary>
    public bool TypingUser
    {
        get => typingUser;
        set => this.RaiseAndSetIfChanged(ref typingUser, value);
    }

    public Messaging Messaging { get; set; } = new Messaging();
    public DisplayPictureReceiving DisplayPictureReceiving { get; set; } = new DisplayPictureReceiving();
    public ContactActions? ContactActions { get; init; }
    public static NotificationHandler? NotificationHandler { get; set; }

    public event EventHandler? DisplayPictureUpdated;

    public Conversation(Contact contact, User user, Database? database = null)
    {
        Contact = contact;
        User = user;
        Database = database;

        DisplayPicture? displayPicture = Database?.GetContactDisplayPicture(Contact.Email);
        if (displayPicture != null)
        {
            using MemoryStream pictureStream = new MemoryStream(displayPicture.PictureData);
            Contact.DisplayPicture = new Bitmap(pictureStream);
            Contact.DisplayPictureHash = displayPicture.PictureHash;
        }
        DisplayPictureReceiving.DisplayPictureUpdated += DisplayPictureReceiving_DisplayPictureUpdated;

        // Load only last four messages, the user needs to click on the menu item to show more
        GetHistory(4);
    }

    /// <summary>
    /// Opens a new conversation window or activates one if it's already open.
    /// </summary>
    public void OpenWindow()
    {
        if (conversationWindow != null)
            conversationWindow.Activate();
        else
        {
            conversationWindow = new ConversationWindow
            {
                DataContext = new ConversationWindowViewModel
                {
                    Conversation = this
                }
            };

            conversationWindow.Closed += ConversationWindow_Closed;
            conversationWindow.Activated += ConversationWindow_Activated;
            conversationWindow.Show();
        }
    }

    /// <summary>
    /// Closes the conversation window.
    /// </summary>
    public void CloseWindow()
    {
        conversationWindow?.Close();
    }

    private void ConversationWindow_Activated(object? sender, EventArgs e)
    {
        Contact.NewMessages = false;
    }

    private void ConversationWindow_Closed(object? sender, EventArgs e)
    {
        conversationWindow!.Closed -= ConversationWindow_Closed;
        conversationWindow.Activated -= ConversationWindow_Activated;
        conversationWindow = null;
    }

    /// <summary>
    /// Sends a text message and saves it to the database.
    /// </summary>
    public async Task SendTextMessage(TextPlain textMessage)
    {
        if (string.IsNullOrEmpty(textMessage.Text))
            return;

        try
        {
            if (Messaging.Server == null || !Messaging.Server.Connected)
            {
                Messaging.Server = await ContactActions!.Server.SendXFR(Contact.Email, ContactActions!.ContactList);
                SubscribeToEvents();
            }

            await Messaging.SendTextMessage(textMessage);
            TypingUser = false;

            Message message = new Message
            {
                Sender = User.Email,
                SenderDisplayName = User.DisplayName,
                Recipient = Contact.Email,
                RecipientDisplayName = Contact.DisplayName,

                Bold = textMessage.Bold,
                Italic = textMessage.Italic,
                Decorations = textMessage.Decorations,
                Color = textMessage.Color,

                Text = textMessage.Text,
                DateTime = DateTime.Now
            };

            Messages.Add(message);
            if (SettingsManager.Settings.SaveMessagingHistory)
                Database?.SaveMessage(message);
        }

        catch (Exception)
        {
            Messages.Add(new Message
            {
                Text = "One or more messages could not be delivered",
                DateTime = DateTime.Now
            });
        }
    }

    /// <summary>
    /// Calls switchboard method to send typing notifications.
    /// </summary>
    /// <returns></returns>
    public async Task SendTypingUser()
    {
        if (Messaging.Server == null || !Messaging.Server.Connected)
            return;

        await Messaging.SendTypingUser();
    }

    /// <summary>
    /// Sends a nudge message and saves it to the database.
    /// </summary>
    /// <returns></returns>
    public async Task SendNudge()
    {
        try
        {
            if (Messaging.Server == null || !Messaging.Server.Connected)
            {
                Messaging.Server = await ContactActions!.Server.SendXFR(Contact.Email, ContactActions!.ContactList);
                SubscribeToEvents();
            }

            await Messaging.SendNudge();
            TypingUser = false;

            Message message = new Message
            {
                Sender = User.Email,
                Recipient = Contact.Email,
                Text = $"You sent {Contact.DisplayName} a nudge.",
                DateTime = DateTime.Now
            };

            Messages.Add(message);
            if (SettingsManager.Settings.SaveMessagingHistory)
                Database?.SaveMessage(message);
        }

        catch (Exception)
        {
            Messages.Add(new Message
            {
                Text = "One or more messages could not be delivered",
                DateTime = DateTime.Now
            });
        }
    }

    /// <summary>
    /// Retrieves a number of messages from the database. If the count parameter is not set or is 0, all messages are returned.
    /// </summary>
    /// <param name="count">Amount of messages to retrieve. 0 returns all messages.</param>
    public void GetHistory(int count = 0)
    {
        if (Database == null)
            return;

        List<Message> history = Database.GetMessages(Contact.Email, User.Email);
        foreach (Message message in history)
        {
            message.IsHistory = true;
        }

        if (count > 0)
            history = history.Skip(history.Count - count).ToList();

        MessageHistory = new ObservableCollection<Message>(history);
    }

    /// <summary>
    /// Deletes from the database and memory all messages between a user and contact.
    /// </summary>
    public void DeleteHistory()
    {
        Database?.DeleteMessages(User.Email, Contact.Email);
        MessageHistory = null;
    }

    /// <summary>
    /// Disconnects from the switchboard.
    /// </summary>
    /// <returns></returns>
    public async Task Disconnect()
    {
        if (Messaging.Server == null)
            return;

        await Messaging.Server.DisconnectAsync();
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// Subscribes to switchboard events.
    /// </summary>
    public void SubscribeToEvents()
    {
        if (Messaging.Server != null)
            Messaging.Server.Disconnected += Switchboard_Disconnected;

        if (Messaging.IncomingMessaging != null)
            Messaging.IncomingMessaging.MessageReceived += IncomingMessaging_MessageReceived;
    }

    /// <summary>
    /// Unsubscribes from switchboard events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (Messaging.Server != null)
            Messaging.Server.Disconnected -= Switchboard_Disconnected;

        if (Messaging.IncomingMessaging != null)
            Messaging.IncomingMessaging.MessageReceived -= IncomingMessaging_MessageReceived;
    }

    private void Switchboard_Disconnected(object? sender, DisconnectedEventArgs e)
    {
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// Replaces the switchboard in case a new session with the same contact is now being used.
    /// </summary>
    public async void NotificationServer_SwitchboardChanged(object? sender, SwitchboardEventArgs e)
    {
        if (Contact.Email != e.Switchboard?.Contact?.Email)
            return;

        await Disconnect();
        Messaging.Server = e.Switchboard;
        Messaging.StartIncoming();
        SubscribeToEvents();
    }

    /// <summary>
    /// If the new message is a typing notification, shows the "is writing..." text for 6 seconds. Otherwise, adds it to message list and database.
    /// </summary>
    private async void IncomingMessaging_MessageReceived(object? sender, MessageEventArgs e)
    {
        if (e.TypingUser)
        {
            TypingUser = true;

            await Task.Delay(6000);
            TypingUser = false;
            return;
        }

        if (string.IsNullOrEmpty(e.Message?.Text))
            return;

        TypingUser = false;

        Message message = new Message
        {
            Sender = Contact.Email,
            Recipient = User.Email,
            Bold = e.Message.Bold,
            Italic = e.Message.Italic,
            Color = e.Message.Color,
            Text = e.Message?.Text!,
            DateTime = DateTime.Now,
            IsNudge = e.IsNudge
        };

        if (e.Message!.Strikethrough)
            message.Decorations = "Strikethrough";

        if (e.Message.Underline)
            message.Decorations += " Underline";

        // Nudges include sender display names in the message itself
        if (!message.IsNudge)
        {
            message.SenderDisplayName = Contact.DisplayName;
            message.RecipientDisplayName = User.DisplayName;
        }

        Messages.Add(message);

        if (SettingsManager.Settings.SaveMessagingHistory)
            Database?.SaveMessage(message);

        if (conversationWindow == null || !conversationWindow.IsActive)
            Contact.NewMessages = true;

        if (User.Presence != PresenceStatus.GetFullName(PresenceStatus.Busy))
        {
            if (conversationWindow == null || !conversationWindow.IsActive)
            {
                NotificationHandler?.PlaySound();
                NotificationHandler.ShowNativeNotification(message);
            }

            if (NotificationHandler != null)
                await NotificationHandler.InvokeNotification(Contact, message);
        }
    }

    /// <summary>
    /// When a display picture is received, sets it in the UI and saves it to the database.
    /// </summary>
    private void DisplayPictureReceiving_DisplayPictureUpdated(object? sender, DisplayPictureEventArgs e)
    {
        if (e.DisplayPicture == null)
            return;

        using MemoryStream pictureStream = new MemoryStream(e.DisplayPicture);
        Contact.DisplayPicture = new Bitmap(pictureStream);
        Contact.DisplayPictureHash = e.DisplayPictureHash;

        Database?.SaveDisplayPicture(new DisplayPicture
        {
            ContactEmail = Contact.Email,
            PictureData = pictureStream.ToArray(),
            PictureHash = e.DisplayPictureHash!
        });

        DisplayPictureUpdated?.Invoke(this, EventArgs.Empty);
    }
}
