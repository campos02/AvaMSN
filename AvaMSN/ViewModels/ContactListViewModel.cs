using ReactiveUI;
using System.Reactive;
using System.Collections.ObjectModel;
using AvaMSN.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.IO;
using AvaMSN.MSNP.Exceptions;
using AvaMSN.Views;
using System.Collections.Generic;
using AvaMSN.MSNP.Utils;
using AvaMSN.Utils;
using SkiaSharp;
using Contact = AvaMSN.Models.Contact;
using ContactEventArgs = AvaMSN.Models.ContactEventArgs;
using Profile = AvaMSN.Models.Profile;

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

    private Contact? selectedContact;

    public Contact? SelectedContact
    {
        get => selectedContact;
        set => this.RaiseAndSetIfChanged(ref selectedContact, value);
    }

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

        if (NotificationHandler != null)
        {
            NotificationHandler.ReplyTapped += NotificationHandler_ReplyTapped;
            NotificationHandler.ApplicationExit += NotificationHandler_ApplicationExit;
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
            Title = "Select display picture",
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        if (files.Count > 0)
        {
            // Scale to height
            int pictureSize = 100;
            await using Stream fileStream = await files[0].OpenReadAsync();
            Bitmap picture = Bitmap.DecodeToHeight(fileStream, pictureSize);
            using MemoryStream pictureStream = new MemoryStream();
            picture.Save(pictureStream);
            pictureStream.Seek(0, SeekOrigin.Begin);

            // Crop to width
            int cropStart = (picture.PixelSize.Width - pictureSize) / 2;
            int cropEnd = (picture.PixelSize.Width + pictureSize) / 2;

            using SKBitmap bitmap = SKBitmap.Decode(pictureStream);
            SKRect cropRect = new SKRect(cropStart, 0, cropEnd, pictureSize);
            using SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width,
                                                  (int)cropRect.Height);
            SKRect destination = new SKRect(0, 0, cropRect.Width, cropRect.Height);
            SKRect source = new SKRect(cropRect.Left, cropRect.Top,
                                       cropRect.Right, cropRect.Bottom);

            using SKCanvas canvas = new SKCanvas(croppedBitmap);
            canvas.DrawBitmap(bitmap, source, destination);

            // Set and save cropped picture
            using MemoryStream croppedPictureStream = new MemoryStream();
            croppedBitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(croppedPictureStream);
            croppedPictureStream.Seek(0, SeekOrigin.Begin);
            Profile.DisplayPicture = new Bitmap(croppedPictureStream);

            Database?.DeleteUserDisplayPictures(Profile.Email);
            Database?.SaveDisplayPicture(new DisplayPicture()
            {
                ContactEmail = Profile.Email,
                PictureData = croppedPictureStream.ToArray(),
                IsUserPicture = true
            });

            if (NotificationServer == null)
                return;

            NotificationServer.Profile.DisplayPicture = croppedPictureStream.ToArray();
            NotificationServer.CreateMSNObject();

            // Flash presence because otherwise the new picture isn't broadcast
            string oldPresence = NotificationServer.Profile.Presence;
            if (oldPresence != PresenceStatus.Invisible)
            {
                NotificationServer.Profile.Presence = PresenceStatus.Idle;
                await NotificationServer.SendCHG();

                NotificationServer.Profile.Presence = oldPresence;
                await NotificationServer.SendCHG();
            }
        }
    }

    private async Task AddContact()
    {
        if (NewContactDisplayName == string.Empty)
            NewContactDisplayName = NewContactEmail;

        if (NotificationServer == null)
            return;

        await NotificationServer.AddContact(NewContactEmail, NewContactDisplayName);

        foreach (ContactGroup group in ListData.ContactGroups!)
        {
            if (group.Name == "Offline")
            {
                group.Contacts.Add(new Contact
                {
                    Email = NewContactEmail,
                    DisplayName = NewContactDisplayName,
                    Presence = PresenceStatus.GetFullName(PresenceStatus.Offline),
                    PresenceColor = ContactListData.GetStatusColor(PresenceStatus.Offline)
                });
            }
        }

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

        Conversation? conversation = Conversations.LastOrDefault(conv => conv.Contact == SelectedContact);
        MSNP.Utils.Contact? contact = NotificationServer.ContactService.Contacts.FirstOrDefault(c => c.Email == SelectedContact.Email) ??
                                      throw new ContactException("Could not find the selected contact");

        if (conversation == null)
        {
            conversation = new Conversation(SelectedContact, Profile, Database)
            {
                NotificationServer = NotificationServer
            };

            conversation.DisplayPictureUpdated += Conversation_DisplayPictureUpdated;
            NotificationServer.SwitchboardChanged += conversation.NotificationServer_SwitchboardChanged;

            Conversations.Add(conversation);
        }
        
        conversation.OpenWindow();

        if (conversation.Switchboard == null || !conversation.Switchboard.Connected)
        {
            if (SelectedContact.Presence != PresenceStatus.GetFullName(PresenceStatus.Offline))
            {
                conversation.Switchboard = await NotificationServer.SendXFR(contact);
                conversation.SubscribeToEvents();
            }
        }
        
        try
        {
            if (contact.DisplayPictureHash != SelectedContact.DisplayPictureHash && conversation.Switchboard != null)
                await conversation.Switchboard.GetDisplayPicture();
        }
        catch (OperationCanceledException) { return; }
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
    /// Unsubscribes from notification server events.
    /// </summary>
    public void UnsubscribeFromEvents()
    {
        NotificationServer!.Disconnected -= NotificationServer_Disconnected;
        NotificationServer.SwitchboardChanged -= NotificationServer_SwitchboardChanged;
    }

    private async void NotificationHandler_ApplicationExit(object? sender, EventArgs e)
    {
        await SignOut();
    }

    private async void NotificationHandler_ReplyTapped(object? sender, ContactEventArgs e)
    {
        SelectedContact = e.Contact;
        await Chat();
    }

    /// <summary>
    /// If a new switchboard session is taking place and it's not with the current contact, creates a new conversation instance.
    /// </summary>
    public void NotificationServer_SwitchboardChanged(object? sender, SwitchboardEventArgs e)
    {
        if (ContactGroups == null || e.Switchboard == null)
            return;

        Conversation? conversation = Conversations.LastOrDefault(conv => conv.Contact.Email == e.Switchboard.Contact.Email);

        if (conversation == null)
        {
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

            conversation = new Conversation(contact, Profile, Database!)
            {
                Switchboard = e.Switchboard
            };

            conversation.SubscribeToEvents();
            NotificationServer!.SwitchboardChanged += conversation.NotificationServer_SwitchboardChanged;

            Conversations.Add(conversation);
        }
    }

    /// <summary>
    /// Resets data, closes chat windows and returns to login page when disconnected from the server.
    /// </summary>
    /// <exception cref="ConnectionException">Thrown if the disconnection wasn't requested.</exception>
    public async void NotificationServer_Disconnected(object? sender, DisconnectedEventArgs e)
    {
        // Close every conversation
        foreach (Conversation conversation in Conversations)
        {
            await conversation.Disconnect();
            NotificationServer!.SwitchboardChanged -= conversation.NotificationServer_SwitchboardChanged;
            conversation.DisplayPictureUpdated -= Conversation_DisplayPictureUpdated;

            conversation.CloseWindow();
        }

        Conversations = [];
        NotificationServer = null;
        SelectedContact = null;
        ListData = new();

        Disconnected?.Invoke(this, EventArgs.Empty);

        if (!e.Requested)
            NotificationHandler?.ShowError("Lost connection to the server");
    }

    /// <summary>
    /// Updates picture data in every contact group.
    /// </summary>
    private void Conversation_DisplayPictureUpdated(object? sender, EventArgs e)
    {
        if (ContactGroups == null)
            return;

        if (sender is Conversation conversation)
        {
            foreach (ContactGroup group in ContactGroups)
            {
                Contact? contact = group.Contacts.FirstOrDefault(contact => contact.Email == conversation?.Contact.Email);

                if (contact != null)
                    contact.DisplayPicture = conversation.Contact.DisplayPicture!;
            }
        }
    }
}
