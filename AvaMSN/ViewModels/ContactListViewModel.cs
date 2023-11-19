using ReactiveUI;
using System.Reactive;
using System.Collections.ObjectModel;
using AvaMSN.Models;
using AvaMSN.MSNP.PresenceStatus;
using System.Threading.Tasks;
using System;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.IO;
using Avalonia;

namespace AvaMSN.ViewModels;

public class ContactListViewModel : ViewModelBase
{
    public ReactiveCommand<string, Unit> ChangePresenceCommand { get; }

    public ReactiveCommand<Unit, Unit> SignOutCommand { get; }
    public ReactiveCommand<Unit, Unit> ChangeNameCommand { get; }
    public ReactiveCommand<Unit, Unit> ChangePersonalMessageCommand { get; }
    public ReactiveCommand<Unit, Unit> ChatCommand { get; }
    public ReactiveCommand<Unit, Unit> OptionsCommand { get; }

    public ReactiveCommand<Unit, Unit> AddContactCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveContactCommand { get; }
    public ReactiveCommand<Unit, Unit> BlockContactCommand { get; }
    public ReactiveCommand<Unit, Unit> UnblockContactCommand { get; }

    public ContactListData ListData { get; set; } = new();
    public ObservableCollection<ContactGroup>? ContactGroups => ListData.ContactGroups;
    public static Presence[] Statuses => ContactListData.Statuses;

    public Contact? SelectedContact { get; set; }
    public Conversation? CurrentConversation { get; set; }

    public Profile Profile
    {
        get => ListData.Profile;
        set => ListData.Profile = value;
    }

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

    public string NewContactEmail { get; set; } = string.Empty;
    public string NewContactDisplayName { get; set; } = string.Empty;

    public Database? Database { get; set; }

    public event EventHandler? Disconnected;
    public event EventHandler? OptionsButtonPressed;

    public event EventHandler? ChatStarted;
    public event EventHandler<NewMessageEventArgs>? NewMessage;

    public ContactListViewModel()
    {
        ChangePresenceCommand = ReactiveCommand.CreateFromTask<string>(ChangePresence);

        SignOutCommand = ReactiveCommand.CreateFromTask(SignOut);
        ChangeNameCommand = ReactiveCommand.CreateFromTask(ChangeName);
        ChangePersonalMessageCommand = ReactiveCommand.CreateFromTask(ChangePersonalMessage);
        ChatCommand = ReactiveCommand.CreateFromTask(Chat);
        OptionsCommand = ReactiveCommand.Create(Options);

        AddContactCommand = ReactiveCommand.CreateFromTask(AddContact);
        RemoveContactCommand = ReactiveCommand.CreateFromTask(RemoveContact);
        BlockContactCommand = ReactiveCommand.CreateFromTask(BlockContact);
        UnblockContactCommand = ReactiveCommand.CreateFromTask(UnblockContact);
    }

    private async Task ChangePresence(string presence)
    {
        if (NotificationServer == null)
            return;

        Profile.Presence = PresenceStatus.GetFullName(presence);

        NotificationServer.Profile.Presence = presence;
        await NotificationServer.SendCHG();

        SelectedOptionIndex = -1;
        SelectedOptionIndex = 0;
    }

    private async Task ChangeName()
    {
        if (NotificationServer == null)
            return;

        Profile.DisplayName = DisplayName;

        NotificationServer.Profile.DisplayName = DisplayName;
        await NotificationServer.ChangeDisplayName();

        SelectedOptionIndex = -1;
        SelectedOptionIndex = 0;
    }

    private async Task ChangePersonalMessage()
    {
        if (NotificationServer == null)
            return;

        NotificationServer.Profile.PersonalMessage = PersonalMessage;
        Profile.PersonalMessage = PersonalMessage;
        await NotificationServer.SendUUX();

        Database?.SavePersonalMessage(Profile.Email, Profile.PersonalMessage);
    }

    public async Task ChangeDisplayPicture(TopLevel topLevel)
    {
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open display picture",
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        });

        if (files.Count > 0)
        {
            await using Stream fileStream = await files[0].OpenReadAsync();

            // Loading then resizing due to a bug in DecodeToWidth
            // https://github.com/AvaloniaUI/Avalonia/discussions/13239
            Profile.DisplayPicture = new Bitmap(fileStream).CreateScaledBitmap(new PixelSize(96, 96));

            using MemoryStream pictureStream = new MemoryStream();
            Profile.DisplayPicture.Save(pictureStream);

            Database?.SaveDisplayPicture(new DisplayPicture()
            {
                ContactEmail = Profile.Email,
                PictureData = pictureStream.ToArray(),
                IsUserPicture = true
            });

            if (NotificationServer == null)
                return;

            NotificationServer.Profile.DisplayPicture = pictureStream.ToArray();
            NotificationServer.GenerateMSNObject();
            await NotificationServer.SendCHG();
        }
    }

    private async Task AddContact()
    {
        ListData.ContactGroups?[(int)ContactListData.DefaultGroupIndex.Offline].Contacts.Add(new Contact
        {
            Email = NewContactEmail,
            DisplayName = NewContactDisplayName
        });

        if (NotificationServer == null)
            return;

        await NotificationServer.AddContact(NewContactEmail, NewContactDisplayName);
    }

    private async Task RemoveContact()
    {
        if (SelectedContact == null || ListData.ContactGroups == null)
            return;

        string email = SelectedContact.Email;

        foreach (ContactGroup group in ListData.ContactGroups)
        {
            group.Contacts.Remove(SelectedContact);
        }

        if (NotificationServer == null)
            return;

        await NotificationServer.RemoveContact(email);
    }

    private async Task BlockContact()
    {
        if (SelectedContact == null || NotificationServer == null)
            return;

        await NotificationServer.BlockContact(SelectedContact.Email);
        SelectedContact.Blocked = true;
    }

    private async Task UnblockContact()
    {
        if (SelectedContact == null || NotificationServer == null)
            return;

        await NotificationServer.UnblockContact(SelectedContact.Email);
        SelectedContact.Blocked = false;
    }

    public async Task Chat()
    {
        if (SelectedContact == null || NotificationServer == null)
            return;

        if (SelectedContact == CurrentConversation?.Contact)
        {
            ChatStarted?.Invoke(this, EventArgs.Empty);
            return;
        }

        MSNP.Contact? contact = NotificationServer.ContactList.Contacts.FirstOrDefault(c => c.Email == SelectedContact.Email) ?? new MSNP.Contact()
        {
            Email = SelectedContact.Email,
            DisplayName = SelectedContact.DisplayName,
            PersonalMessage = SelectedContact.PersonalMessage,
            Presence = SelectedContact.Presence
        };

        CurrentConversation = new Conversation(SelectedContact, Profile, Database!);

        if (SelectedContact.Presence != PresenceStatus.GetFullName(PresenceStatus.Offline))
        {
            MSNP.Switchboard? switchboard = NotificationServer.Switchboards.FirstOrDefault(sb => sb.Contact.Email == contact.Email && sb.Connected);

            if (switchboard == null)
                CurrentConversation.Switchboard = await NotificationServer!.SendXFR(contact);
            else
                CurrentConversation.Switchboard = switchboard;

            CurrentConversation.SubscribeToSwitchboardsEvents();
            CurrentConversation.NewMessage += Conversation_NewMessage;
            CurrentConversation.DisplayPictureUpdated += Conversation_DisplayPictureUpdated;
        }

        NotificationServer.SwitchboardChanged += CurrentConversation.NotificationServer_SwitchboardChanged;

        ChatStarted?.Invoke(this, EventArgs.Empty);
    }

    private void Options()
    {
        OptionsButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private async Task SignOut()
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
            DisplayName = e.Switchboard.Contact.DisplayName,
            Presence = PresenceStatus.GetFullName(e.Switchboard.Contact.Presence)
        };

        foreach (ContactGroup group in ContactGroups)
        {
            contact = group.Contacts.FirstOrDefault(contact => contact.Email == e.Switchboard.Contact.Email) ?? contact;
        }

        if (CurrentConversation?.Contact.Email != contact.Email)
        {
            Conversation conversation = new Conversation(contact, Profile, Database!)
            {
                Switchboard = e.Switchboard
            };

            conversation.SubscribeToSwitchboardsEvents();
            conversation.NewMessage += Conversation_NewMessage;
        }
    }

    public void NotificationServer_Disconnected(object? sender, EventArgs e)
    {
        if (CurrentConversation != null)
        {
            CurrentConversation.UnsubscribeToEvents();
            
            if (NotificationServer != null)
                NotificationServer.SwitchboardChanged -= CurrentConversation!.NotificationServer_SwitchboardChanged;

            CurrentConversation.NewMessage -= Conversation_NewMessage;
            CurrentConversation.DisplayPictureUpdated -= Conversation_DisplayPictureUpdated;
        }

        CurrentConversation = null;
        NotificationServer = null;
        SelectedContact = null;
        ListData = new();

        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private void Conversation_NewMessage(object? sender, NewMessageEventArgs e)
    {
        NewMessage?.Invoke(this, e);
    }

    private void Conversation_DisplayPictureUpdated(object? sender, EventArgs e)
    {
        if (ContactGroups == null)
            return;

        Conversation? conversation = sender as Conversation;

        foreach (ContactGroup group in ContactGroups)
        {
            Contact? contact = group.Contacts.FirstOrDefault(contact => contact.Email == conversation?.Contact.Email);

            if (contact != null)
                contact.DisplayPicture = conversation?.Contact.DisplayPicture;
        }
    }
}
