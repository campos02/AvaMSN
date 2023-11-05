using ReactiveUI;
using System.Reactive;
using System.Collections.ObjectModel;
using AvaMSN.Models;
using AvaMSN.MSNP.PresenceStatus;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace AvaMSN.ViewModels;

public class ContactListViewModel : ConnectedViewModelBase
{
    public ReactiveCommand<string, Unit> ChangePresenceCommand { get; }

    public ReactiveCommand<Unit, Unit> SignOutCommand { get; }
    public ReactiveCommand<Unit, Unit> ChangeNameCommand { get; }
    public ReactiveCommand<Unit, Unit> ChangePersonalMessageCommand { get; }
    public ReactiveCommand<Unit, Unit> ChatCommand { get; }

    public ContactListData ListData { get; set; } = new();
    public ObservableCollection<ContactGroup>? ContactGroups => ListData.ContactGroups;
    public Presence[] Statuses => ContactListData.Statuses;

    public Contact? SelectedContact { get; set; }
    public Conversation? CurrentConversation { get; set; }

    public MSNP.NotificationServer? NotificationServer
    {
        get => ListData.NotificationServer;
        set => ListData.NotificationServer = value;
    }

    private int selectedOptionIndex = 0;

    public int SelectedOptionIndex
    {
        get => selectedOptionIndex;
        set => this.RaiseAndSetIfChanged(ref selectedOptionIndex, value);
    }

    public string DisplayName { get; set; } = string.Empty;
    public string PersonalMessage { get; set; } = string.Empty;

    public Database? Database { get; set; }
    public event EventHandler<NewMessageEventArgs>? NewMessage;

    public ContactListViewModel()
    {
        ChangePresenceCommand = ReactiveCommand.CreateFromTask<string>(ChangePresence);

        SignOutCommand = ReactiveCommand.CreateFromTask(SignOut);
        ChangeNameCommand = ReactiveCommand.CreateFromTask(ChangeName);
        ChangePersonalMessageCommand = ReactiveCommand.CreateFromTask(ChangePersonalMessage);

        ChatCommand = ReactiveCommand.CreateFromTask(Chat);
    }

    public async Task ChangePresence(string presence)
    {
        if (NotificationServer == null)
            return;

        ListData.Profile.Presence = PresenceStatus.GetFullName(presence);

        NotificationServer.ContactList.Profile.Presence = presence;
        await NotificationServer.SendCHG();

        SelectedOptionIndex = -1;
        SelectedOptionIndex = 0;
    }

    public async Task ChangeName()
    {
        if (NotificationServer == null)
            return;

        ListData.Profile.DisplayName = DisplayName;

        NotificationServer.ContactList.Profile.DisplayName = DisplayName;
        await NotificationServer.ChangeDisplayName();

        SelectedOptionIndex = -1;
        SelectedOptionIndex = 0;
    }

    public async Task ChangePersonalMessage()
    {
        if (NotificationServer == null)
            return;

        NotificationServer.ContactList.Profile.PersonalMessage = PersonalMessage;
        ListData.Profile.PersonalMessage = PersonalMessage;
        await NotificationServer.SendUUX();

        Database?.SavePersonalMessage(ListData.Profile.Email, ListData.Profile.PersonalMessage);
    }

    public async Task Chat()
    {
        if (SelectedContact == null
            || NotificationServer == null)
            return;

        if (CurrentConversation != null)
        {
            if (SelectedContact == CurrentConversation.Contact)
            {
                Chatting = true;
                return;
            }
        }

        MSNP.Contact? contact = NotificationServer.ContactList.Contacts.FirstOrDefault(c => c.Email == SelectedContact.Email) ?? new MSNP.Contact()
        {
            Email = SelectedContact.Email,
            DisplayName = SelectedContact.DisplayName,
            PersonalMessage = SelectedContact.PersonalMessage,
            Presence = SelectedContact.Presence
        };

        CurrentConversation = new Conversation(SelectedContact, ListData.Profile, Database!);

        if (SelectedContact.Presence != PresenceStatus.GetFullName(PresenceStatus.Offline))
        {
            MSNP.Switchboard? switchboard = NotificationServer.Switchboards.FirstOrDefault(sb => sb.Contact.Email == contact.Email && sb.Connected);

            if (switchboard == null)
            {
                CurrentConversation.Switchboard = await NotificationServer!.SendXFR(contact);
                await CurrentConversation.Invite();
            }

            else
            {
                CurrentConversation.Switchboard = switchboard;
            }

            CurrentConversation.SubscribeToMessageEvent();
            CurrentConversation.NewMessage += Conversation_NewMessage;
        }

        NotificationServer.SwitchboardChanged += CurrentConversation.NotificationServer_SwitchboardChanged;

        Chatting = true;
    }

    public async Task SignOut()
    {
        if (NotificationServer == null)
            return;

        await NotificationServer.DisconnectAsync();
    }

    public void NotificationServer_SwitchboardChanged(object? sender, MSNP.SwitchboardEventArgs e)
    {
        if (ContactGroups == null || e.Switchboard == null)
            return;

        Contact? contact = new Contact()
        {
            Email = e.Switchboard.Contact.Email,
            DisplayName = e.Switchboard.Contact.DisplayName
        };

        if (CurrentConversation == null || CurrentConversation.Contact == null || CurrentConversation.Contact.Email != contact.Email)
        {
            Conversation conversation = new Conversation(contact, ListData.Profile, Database!)
            {
                Switchboard = e.Switchboard
            };

            conversation.SubscribeToMessageEvent();
            conversation.NewMessage += Conversation_NewMessage;
        }
    }

    public void NotificationServer_Disconnected(object? sender, EventArgs e)
    {
        if (NotificationServer == null)
            return;

        if (CurrentConversation != null)
        {
            CurrentConversation.UnsubscribeToEvents();
            NotificationServer.SwitchboardChanged -= CurrentConversation!.NotificationServer_SwitchboardChanged;
            CurrentConversation.NewMessage -= Conversation_NewMessage;

            CurrentConversation = null;
        }

        NotificationServer = null;
        SelectedContact = null;
        ListData = new();

        LoggedIn = false;
    }

    private void Conversation_NewMessage(object? sender, NewMessageEventArgs e)
    {
        NewMessage?.Invoke(this, e);
    }
}
