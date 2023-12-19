using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using AvaMSN.MSNP;
using AvaMSN.MSNP.Messages;
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

    public Profile Profile { get; private set; }
    public Contact Contact { get; private set; }
    private Database? Database { get; set; }

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

    public Switchboard? Switchboard { get; set; }
    private ConversationWindow? ConversationWindow { get; set; }

    public event EventHandler? DisplayPictureUpdated;
    public event EventHandler<NewMessageEventArgs>? NewMessage;

    public Conversation(Contact contact, Profile profile, Database? database = null)
    {
        Contact = contact;
        Profile = profile;
        Database = database;

        if (Database != null)
        {
            DisplayPicture? displayPicture = Database.GetContactDisplayPicture(Contact.Email);
            if (displayPicture != null)
            {
                using MemoryStream pictureStream = new MemoryStream(displayPicture.PictureData);
                Contact.DisplayPicture = new Bitmap(pictureStream);
            }
        }

        // Load only last four messages by default
        GetHistory(4);
    }

    /// <summary>
    /// Opens a new conversation window or activates one if it's already open.
    /// </summary>
    public void OpenWindow()
    {
        if (ConversationWindow != null)
            ConversationWindow.Activate();
        else
        {
            ConversationWindow = new ConversationWindow()
            {
                DataContext = new ConversationWindowViewModel()
                {
                    Conversation = this
                }
            };

            ConversationWindow.Closed += ConversationWindow_Closed;
            ConversationWindow.Show();
        }
    }

    /// <summary>
    /// Closes the conversation window.
    /// </summary>
    public void CloseWindow()
    {
        ConversationWindow?.Close();
    }

    private void ConversationWindow_Closed(object? sender, EventArgs e)
    {
        ConversationWindow = null;
    }

    /// <summary>
    /// Sends a text message and saves it to the database.
    /// </summary>
    public async Task SendTextMessage(TextPlain textMessage)
    {
        if (Switchboard == null || !Switchboard.Connected || string.IsNullOrEmpty(textMessage.Content))
            return;

        try
        {
            await Switchboard.SendTextMessage(textMessage);

            TypingUser = false;

            Message message = new Message
            {
                Sender = Profile.Email,
                SenderDisplayName = Profile.DisplayName,
                Recipient = Contact.Email,
                RecipientDisplayName = Contact.DisplayName,

                Bold = textMessage.Bold,
                Italic = textMessage.Italic,

                Text = textMessage.Content,
                DateTime = DateTime.Now
            };

            if (textMessage.Strikethrough)
                message.Decorations = "Strikethrough";

            if (textMessage.Underline)
                message.Decorations += " Underline";

            Messages.Add(message);
            
            if (SettingsManager.Settings.SaveMessagingHistory)
                Database?.SaveMessage(message);
        }
        catch (Exception)
        {
            Messages.Add(new Message { Text = "Failed to send one or more messages" });
        }
    }

    /// <summary>
    /// Calls switchboard method to send typing notifications.
    /// </summary>
    /// <returns></returns>
    public async Task SendTypingUser()
    {
        if (Switchboard == null || !Switchboard.Connected)
            return;

        await Switchboard.SendTypingUser();
    }

    /// <summary>
    /// Sends a nudge message and saves it to the database.
    /// </summary>
    /// <returns></returns>
    public async Task SendNudge()
    {
        if (Switchboard == null || !Switchboard.Connected)
            return;

        try
        {
            await Switchboard.SendNudge();
            TypingUser = false;

            Message message = new Message
            {
                Sender = Profile.Email,
                Recipient = Contact.Email,
                Text = $"You sent {Contact.DisplayName} a nudge",
                DateTime = DateTime.Now
            };

            Messages.Add(message);

            if (SettingsManager.Settings.SaveMessagingHistory)
                Database?.SaveMessage(message);
        }
        catch (Exception)
        {
            Messages.Add(new Message { Text = "Failed to send one or more messages" });
        }
    }

    /// <summary>
    /// Retrieves a number of messages from the database. If the count parameter is not set or is 0, all messages are returned.
    /// </summary>
    /// <param name="count">Amount of messages to retrieve. 0 returns all messages</param>
    public void GetHistory(int count = 0)
    {
        if (Database == null)
            return;

        List<Message> history = Database.GetMessages(Contact.Email, Profile.Email);
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
        Database?.DeleteMessages(Profile.Email, Contact.Email);
        MessageHistory = null;
    }

    /// <summary>
    /// Disconnects from switchboard session.
    /// </summary>
    /// <returns></returns>
    public async Task Disconnect()
    {
        if (Switchboard == null)
            return;

        await Switchboard.DisconnectAsync();
    }

    /// <summary>
    /// Subscribes to switchboard events.
    /// </summary>
    public void SubscribeToEvents()
    {
        if (Switchboard == null)
            return;

        Switchboard.MessageReceived += Switchboard_MessageReceived;
        Switchboard.DisplayPictureUpdated += Switchboard_DisplayPictureUpdated;
    }

    /// <summary>
    /// Unsubscribes to switchboard events.
    /// </summary>
    public void UnsubscribeToEvents()
    {
        if (Switchboard == null)
            return;

        Switchboard.MessageReceived -= Switchboard_MessageReceived;
        Switchboard.DisplayPictureUpdated -= Switchboard_DisplayPictureUpdated;
    }

    /// <summary>
    /// Replaces the switchboard in case a new session with the same contact is now being used.
    /// </summary>
    public void NotificationServer_SwitchboardChanged(object? sender, SwitchboardEventArgs e)
    {
        if (Contact.Email != e.Switchboard?.Contact.Email)
            return;

        Switchboard = e.Switchboard;
        SubscribeToEvents();
    }

    /// <summary>
    /// If the new message is a typing notification, shows the "is writing..." text for 6 seconds. Otherwise, adds it to message list and database.
    /// </summary>
    private async void Switchboard_MessageReceived(object? sender, MessageEventArgs e)
    {
        if (e.TypingUser)
        {
            TypingUser = true;

            await Task.Delay(6000);
            TypingUser = false;
        }

        if (string.IsNullOrEmpty(e.Message?.Content))
            return;

        TypingUser = false;

        Message message = new Message
        {
            Sender = Contact.Email,
            Recipient = Profile.Email,

            Bold = e.Message.Bold,
            Italic = e.Message.Italic,

            Text = e.Message?.Content!,
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
            message.RecipientDisplayName = Profile.DisplayName;
        }

        Messages.Add(message);

        if (SettingsManager.Settings.SaveMessagingHistory)
            Database?.SaveMessage(message);

        NewMessage?.Invoke(this, new NewMessageEventArgs()
        {
            Contact = Contact,
            Message = message
        });
    }

    /// <summary>
    /// When a display picture is received, sets it in the interface and saves it to the database.
    /// </summary>
    private void Switchboard_DisplayPictureUpdated(object? sender, DisplayPictureEventArgs e)
    {
        if (e.DisplayPicture == null)
            return;

        using MemoryStream pictureStream = new MemoryStream(e.DisplayPicture);
        Contact.DisplayPicture = new Bitmap(pictureStream);

        Database?.SaveDisplayPicture(new DisplayPicture()
        {
            ContactEmail = Contact.Email,
            PictureData = pictureStream.ToArray()
        });

        DisplayPictureUpdated?.Invoke(this, EventArgs.Empty);
    }
}
