using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using AvaMSN.MSNP;
using AvaMSN.MSNP.Messages;
using ReactiveUI;

namespace AvaMSN.Models;

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

    public bool TypingUser
    {
        get => typingUser;
        set => this.RaiseAndSetIfChanged(ref typingUser, value);
    }

    public Switchboard? Switchboard { get; set; }

    public event EventHandler<NewMessageEventArgs>? NewMessage;
    public event EventHandler? DisplayPictureUpdated;

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

        GetHistory(4);
    }

    public async Task SendTextMessage(string messageText)
    {
        if (Switchboard == null || Profile == null || Contact == null || string.IsNullOrEmpty(messageText))
            return;

        try
        {
            await Switchboard.SendTextMessage(new TextPlain()
            {
                FontName = "Segoe UI",
                Content = messageText
            });

            TypingUser = false;

            Message message = new Message
            {
                Sender = Profile.Email,
                SenderDisplayName = Profile.DisplayName,
                Recipient = Contact.Email,
                RecipientDisplayName = Contact.DisplayName,
                Text = messageText,
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

    public async Task SendTypingUser()
    {
        if (Switchboard == null)
            return;

        await Switchboard.SendTypingUser();
    }

    public async Task SendNudge()
    {
        if (Switchboard == null || Profile == null || Contact == null)
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

    public void DeleteHistory()
    {
        Database?.DeleteMessages(Profile.Email, Contact.Email);
        MessageHistory = null;
    }

    public async Task Disconnect()
    {
        if (Switchboard == null)
            return;

        await Switchboard.DisconnectAsync();
    }

    public void SubscribeToSwitchboardsEvents()
    {
        if (Switchboard == null)
            return;

        Switchboard.MessageReceived += Switchboard_MessageReceived;
        Switchboard.DisplayPictureUpdated += Switchboard_DisplayPictureUpdated;
    }

    public void UnsubscribeToEvents()
    {
        if (Switchboard == null)
            return;

        Switchboard.MessageReceived -= Switchboard_MessageReceived;
    }

    public void NotificationServer_SwitchboardChanged(object? sender, SwitchboardEventArgs e)
    {
        if (Contact.Email != e.Switchboard?.Contact.Email)
            return;

        Switchboard = e.Switchboard;
        SubscribeToSwitchboardsEvents();
    }

    private async void Switchboard_MessageReceived(object? sender, MessageEventArgs e)
    {
        if (e.TypingUser)
        {
            TypingUser = true;

            await Task.Delay(6000);
            TypingUser = false;
        }

        if (e.Message == string.Empty)
            return;

        TypingUser = false;

        Message message = new Message
        {
            Sender = Contact.Email,
            Recipient = Profile.Email,
            Text = e.Message,
            DateTime = DateTime.Now,
            IsNudge = e.IsNudge
        };

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
            Sender = Contact,
            Message = message
        });
    }

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
