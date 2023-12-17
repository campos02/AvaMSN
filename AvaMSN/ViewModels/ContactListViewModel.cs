﻿using ReactiveUI;
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
using AvaMSN.MSNP.Exceptions;
using AvaMSN.Views;
using System.Collections.Generic;

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
    public List<Conversation> Conversations
    {
        get => ListData.Conversations;
        set => ListData.Conversations = value;
    }

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

    public ContactListViewModel()
    {
        ChangePresenceCommand = ReactiveCommand.CreateFromTask<string>(ChangePresence);
        ChangeNameCommand = ReactiveCommand.CreateFromTask(ChangeDisplayName);
        ChangePersonalMessageCommand = ReactiveCommand.CreateFromTask(ChangePersonalMessage);

        ChatCommand = ReactiveCommand.CreateFromTask(Chat);
        OptionsCommand = ReactiveCommand.Create(OpenOptions);

        AddContactCommand = ReactiveCommand.CreateFromTask(AddContact);
        RemoveContactCommand = ReactiveCommand.CreateFromTask(RemoveContact);
        BlockContactCommand = ReactiveCommand.CreateFromTask(BlockContact);
        UnblockContactCommand = ReactiveCommand.CreateFromTask(UnblockContact);

        SignOutCommand = ReactiveCommand.CreateFromTask(SignOut);

        if (NotificationManager != null)
        {
            NotificationManager.ReplyTapped += NotificationManager_ReplyTapped;
            NotificationManager.ApplicationExit += NotificationManager_ApplicationExit;
        }
            
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

    private async Task ChangeDisplayName()
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

            // Load then resize due to a bug in DecodeToWidth
            // https://github.com/AvaloniaUI/Avalonia/discussions/13239
            Profile.DisplayPicture = new Bitmap(fileStream).CreateScaledBitmap(new PixelSize(96, 96));

            using MemoryStream pictureStream = new MemoryStream();
            Profile.DisplayPicture.Save(pictureStream);

            Database?.DeleteUserDisplayPictures(Profile.Email);
            Database?.SaveDisplayPicture(new DisplayPicture()
            {
                ContactEmail = Profile.Email,
                PictureData = pictureStream.ToArray(),
                IsUserPicture = true
            });

            if (NotificationServer == null)
                return;

            NotificationServer.Profile.DisplayPicture = pictureStream.ToArray();
            NotificationServer.CreateMSNObject();
            await NotificationServer.SendCHG();
        }
    }

    private async Task AddContact()
    {
        if (NewContactDisplayName == string.Empty)
            NewContactDisplayName = NewContactEmail;

        if (NotificationServer == null)
            return;

        await NotificationServer.AddContact(NewContactEmail, NewContactDisplayName);

        ListData.ContactGroups?[(int)ContactListData.DefaultGroupIndex.Offline].Contacts.Add(new Contact
        {
            Email = NewContactEmail,
            DisplayName = NewContactDisplayName,
            Presence = PresenceStatus.GetFullName(PresenceStatus.Offline),
            PresenceColor = ContactListData.GetStatusColor(PresenceStatus.Offline)
        });

        NewContactEmail = string.Empty;
        NewContactDisplayName = string.Empty;
    }

    private async Task RemoveContact()
    {
        if (SelectedContact == null || ListData.ContactGroups == null)
            return;

        string email = SelectedContact.Email;

        if (NotificationServer == null)
            return;

        await NotificationServer.RemoveContact(email);

        foreach (ContactGroup group in ListData.ContactGroups)
        {
            group.Contacts.Remove(SelectedContact);
        }
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

    /// <summary>
    /// Creates a new conversation object with the selected contact and opens a new conversation window.
    /// If a conversation with the selected contact already exists however, simply opens its window.
    /// </summary>
    public async Task Chat()
    {
        if (SelectedContact == null || NotificationServer == null)
            return;

        // Unbold contact name
        SelectedContact.NewMessages = false;
        Conversation? conversation = Conversations.FirstOrDefault(conv => conv.Contact == SelectedContact);

        if (conversation == null || conversation.Switchboard == null)
        {
            MSNP.Contact? contact = NotificationServer.ContactList.Contacts.FirstOrDefault(c => c.Email == SelectedContact.Email) ?? new MSNP.Contact()
            {
                Email = SelectedContact.Email,
                DisplayName = SelectedContact.DisplayName,
                PersonalMessage = SelectedContact.PersonalMessage,
                Presence = SelectedContact.Presence
            };

            conversation = new Conversation(SelectedContact, Profile, Database);

            if (SelectedContact.Presence != PresenceStatus.GetFullName(PresenceStatus.Offline))
            {
                // Check if a switchboard session is already open before requesting a new one
                MSNP.Switchboard? switchboard = NotificationServer.Switchboards.FirstOrDefault(sb => sb.Contact.Email == contact.Email && sb.Connected);

                if (switchboard == null)
                    conversation.Switchboard = await NotificationServer!.SendXFR(contact);
                else
                    conversation.Switchboard = switchboard;

                conversation.NewMessage += Conversation_NewMessage;
                conversation.SubscribeToEvents();
                conversation.DisplayPictureUpdated += Conversation_DisplayPictureUpdated;
            }

            NotificationServer.SwitchboardChanged += conversation.NotificationServer_SwitchboardChanged;
            conversation.OpenWindow();
            Conversations.Add(conversation);
        }

        else
            conversation.OpenWindow();
    }

    private void Conversation_NewMessage(object? sender, NewMessageEventArgs e)
    {
        NotificationManager?.InvokeNotification(e.Contact, e.Message);
    }

    /// <summary>
    /// Opens the settings window or activates it if it's already open.
    /// </summary>
    private static void OpenOptions()
    {
        if (SettingsWindow != null)
            SettingsWindow.Activate();
        else
        {
            SettingsWindow = new SettingsWindow()
            {
                DataContext = new SettingsWindowViewModel()
            };

            SettingsWindow.Closed += SettingsWindow_Closed;
            SettingsWindow.Show();
        }
    }

    private static void SettingsWindow_Closed(object? sender, EventArgs e)
    {
        SettingsWindow = null;
    }

    private async Task SignOut()
    {
        if (NotificationServer == null)
            return;

        // Will also invoke the disconnection event
        await NotificationServer.DisconnectAsync();
    }

    /// <summary>
    /// Subscribes to notification server events.
    /// </summary>
    public void SubscribeToEvents()
    {
        NotificationServer!.Disconnected += NotificationServer_Disconnected;
        NotificationServer.SwitchboardChanged += NotificationServer_SwitchboardChanged;
    }

    /// <summary>
    /// Unsubscribes to notification server events.
    /// </summary>
    public void UnsubscribeToEvents()
    {
        NotificationServer!.Disconnected -= NotificationServer_Disconnected;
        NotificationServer.SwitchboardChanged -= NotificationServer_SwitchboardChanged;
    }

    private async void NotificationManager_ApplicationExit(object? sender, EventArgs e)
    {
        await SignOut();
    }

    private async void NotificationManager_ReplyTapped(object? sender, ContactEventArgs e)
    {
        SelectedContact = e.Contact;
        await Chat();
    }

    /// <summary>
    /// If a new switchboard session is taking place and it's not with the current contact, creates a new conversation instance.
    /// </summary>
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

        Conversation? conversation = Conversations.FirstOrDefault(conv => conv.Contact.Email == contact.Email);

        if (conversation == null)
        {
            conversation = new Conversation(contact, Profile, Database!)
            {
                Switchboard = e.Switchboard
            };

            conversation.NewMessage += Conversation_NewMessage;
            NotificationServer!.SwitchboardChanged += conversation.NotificationServer_SwitchboardChanged;
            conversation.SubscribeToEvents();
        }
    }

    /// <summary>
    /// Resets data, closes chat windows and returns to login page when disconnected from the server.
    /// </summary>
    /// <exception cref="ConnectionException">Thrown if the disconnection wasn't requested.</exception>
    public void NotificationServer_Disconnected(object? sender, MSNP.DisconnectedEventArgs e)
    {
        // Close every conversation
        foreach (Conversation conversation in Conversations)
        {
            conversation.UnsubscribeToEvents();
            NotificationServer!.SwitchboardChanged -= conversation.NotificationServer_SwitchboardChanged;
            conversation.DisplayPictureUpdated -= Conversation_DisplayPictureUpdated;
            conversation.NewMessage -= Conversation_NewMessage;

            conversation.CloseWindow();
        }

        Conversations = [];
        NotificationServer = null;
        SelectedContact = null;
        ListData = new();

        Disconnected?.Invoke(this, EventArgs.Empty);
        
        if (!e.Requested)
            throw new ConnectionException("Lost connection to the server");
    }

    /// <summary>
    /// Updates picture data in every contact group.
    /// </summary>
    private void Conversation_DisplayPictureUpdated(object? sender, EventArgs e)
    {
        if (ContactGroups == null)
            return;

        Conversation? conversation = sender as Conversation;

        foreach (ContactGroup group in ContactGroups)
        {
            Contact? contact = group.Contacts.FirstOrDefault(contact => contact.Email == conversation?.Contact.Email);

            if (contact != null)
                contact.DisplayPicture = conversation?.Contact.DisplayPicture!;
        }
    }
}
