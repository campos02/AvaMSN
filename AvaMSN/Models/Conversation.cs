using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaMSN.MSNP;
using ReactiveUI;

namespace AvaMSN.Models;

public class Conversation : ReactiveObject
{
    public Profile Profile { get; private set; }
    public Contact Contact { get; private set; }
    private Database Database { get; set; }

    public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
    public ObservableCollection<Message> MessageHistory { get; set; }

    private bool typingUser;

    public bool TypingUser
    {
        get => typingUser;
        set => this.RaiseAndSetIfChanged(ref typingUser, value);
    }

    public Switchboard? Switchboard { get; set; }

    public event EventHandler<NewMessageEventArgs>? NewMessage;

    public Conversation(Contact contact, Profile profile, Database database)
    {
        Contact = contact;
        Profile = profile;
        Database = database;

        List<Message> history = Database.GetMessages(Contact.Email, Profile.Email);

        foreach (Message message in history)
        {
            message.IsHistory = true;
        }

        MessageHistory = new ObservableCollection<Message>(history.Skip(history.Count - 4));
    }

    public async Task Invite()
    {
        if (Switchboard == null)
            return;

        await Switchboard.SendCAL();
    }

    public async Task SendTextMessage(string messageText)
    {
        if (Switchboard == null || Profile == null || Contact == null || string.IsNullOrEmpty(messageText))
            return;

        try
        {
            await Switchboard.SendTextMessage(messageText);
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
            Database?.SaveMessage(message);
        }
        catch (Exception)
        {
            Messages.Add(new Message { Text = "Failed to send one or more messages" });
        }
    }

    public async Task Disconnect()
    {
        if (Switchboard == null)
            return;

        await Switchboard.DisconnectAsync();
    }

    public void SubscribeToMessageEvent()
    {
        if (Switchboard == null)
            return;

        Switchboard.MessageReceived += Switchboard_MessageReceived;
    }

    public void UnsubscribeToEvents()
    {
        if (Switchboard == null)
            return;

        Switchboard.MessageReceived -= Switchboard_MessageReceived;
    }

    public async void NotificationServer_SwitchboardChanged(object? sender, SwitchboardEventArgs e)
    {
        if (Switchboard == null || e.Switchboard == null || Contact.Email != e.Switchboard.Contact.Email)
            return;

        if (Switchboard.Connected)
            await Switchboard.DisconnectAsync();

        Switchboard = e.Switchboard;
        SubscribeToMessageEvent();
    }

    private async void Switchboard_MessageReceived(object? sender, MessageEventArgs e)
    {
        if (Contact == null || Profile == null)
            return;

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
        Database?.SaveMessage(message);

        NewMessage?.Invoke(this, new NewMessageEventArgs()
        {
            Sender = Contact,
            Message = message
        });
    }
}
