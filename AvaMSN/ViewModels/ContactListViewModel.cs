using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using AvaMSN.Models;
using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.Models;
using AvaMSN.MSNP.NotificationServer.Contacts;
using AvaMSN.MSNP.NotificationServer.UserProfile;
using AvaMSN.Utils;
using AvaMSN.Views;
using ReactiveUI;
using SkiaSharp;
using Contact = AvaMSN.Models.Contact;
using ContactEventArgs = AvaMSN.Models.ContactEventArgs;
using User = AvaMSN.Models.User;

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

    public ContactListData ListData { get; protected set; } = new ContactListData();
    public ObservableCollection<ContactGroup>? ContactGroups => ListData.ContactGroups;
    public static Presence[] Statuses => ContactListData.Statuses;

    private Contact? selectedContact;

    public Contact? SelectedContact
    {
        get => selectedContact;
        set => this.RaiseAndSetIfChanged(ref selectedContact, value);
    }

    private List<Conversation> Conversations
    {
        get => ListData.Conversations;
        set => ListData.Conversations = value;
    }

    public User User
    {
        get => ListData.User;
        set => ListData.User = value;
    }

    public ContactActions? ContactActions
    {
        get => ListData.ContactActions;
        set => ListData.ContactActions = value;
    }

    public UserProfile? UserProfile
    {
        get => ListData.UserProfile;
        set => ListData.UserProfile = value;
    }

    private int selectedOptionIndex;

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
    
    public event EventHandler<Models.DisconnectedEventArgs>? Disconnected;

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
        if (UserProfile == null || UserProfile.User == null)
            return;

        User.Presence = PresenceStatus.GetFullName(presence);
        UserProfile.User.Presence = presence;
        await UserProfile.SendCHG();

        SelectedOptionIndex = -1;
        SelectedOptionIndex = 0;
    }

    private async Task ChangeDisplayName()
    {
        if (UserProfile == null || UserProfile.User == null)
            return;

        User.DisplayName = DisplayName;
        UserProfile.User.DisplayName = DisplayName;
        await UserProfile.ChangeDisplayName();

        SelectedOptionIndex = -1;
        SelectedOptionIndex = 0;
    }

    private async Task ChangePersonalMessage()
    {
        if (UserProfile == null || UserProfile.User == null)
            return;

        UserProfile.User.PersonalMessage = PersonalMessage;
        User.PersonalMessage = PersonalMessage;
        await UserProfile.SendUUX();

        Database?.SavePersonalMessage(User.Email, User.PersonalMessage);
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
            int pictureSize = 100;
            await using Stream fileStream = await files[0].OpenReadAsync();
            using SKBitmap bitmap = SKBitmap.Decode(fileStream);

            // Resize while maintaining aspect ratio
            double aspectRatio;
            int newHeight, newWidth;
            if (bitmap.Height <= bitmap.Width)
            {
                aspectRatio = (double)bitmap.Width / bitmap.Height;
                newHeight = pictureSize;
                newWidth = (int)Math.Round(pictureSize * aspectRatio);
            }
            else
            {
                aspectRatio = (double)bitmap.Height / bitmap.Width;
                newWidth = pictureSize;
                newHeight = (int)Math.Round(pictureSize * aspectRatio);
            }
            using SKBitmap resized = bitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);

            // Crop to width and height
            int verticalCropStart = (resized.Height - pictureSize) / 2;
            int verticalCropEnd = (resized.Height + pictureSize) / 2;
            int horizontalCropStart = (resized.Width - pictureSize) / 2;
            int horizontalCropEnd = (resized.Width + pictureSize) / 2;

            SKRect cropRect = new SKRect(horizontalCropStart, verticalCropStart, horizontalCropEnd, verticalCropEnd);
            using SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width,
                                                  (int)cropRect.Height);
            SKRect destination = new SKRect(0, 0, cropRect.Width, cropRect.Height);
            SKRect source = new SKRect(cropRect.Left, cropRect.Top,
                                       cropRect.Right, cropRect.Bottom);

            using SKCanvas canvas = new SKCanvas(croppedBitmap);
            canvas.DrawBitmap(resized, source, destination);

            // Set and save cropped picture
            using MemoryStream croppedPictureStream = new MemoryStream();
            croppedBitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(croppedPictureStream);
            croppedPictureStream.Seek(0, SeekOrigin.Begin);
            User.DisplayPicture = new Bitmap(croppedPictureStream);

            Database?.DeleteUserDisplayPictures(User.Email);
            Database?.SaveDisplayPicture(new DisplayPicture
            {
                ContactEmail = User.Email,
                PictureData = croppedPictureStream.ToArray(),
                IsUserPicture = true
            });

            if (UserProfile == null || UserProfile.User == null)
                return;

            UserProfile.User.DisplayPicture = croppedPictureStream.ToArray();
            UserProfile.CreateMSNObject();

            // Flash presence because otherwise the new picture isn't broadcast
            string oldPresence = UserProfile.User.Presence;
            if (oldPresence != PresenceStatus.Invisible)
            {
                UserProfile.User.Presence = PresenceStatus.Idle;
                await UserProfile.SendCHG();

                UserProfile.User.Presence = oldPresence;
                await UserProfile.SendCHG();
            }
        }
    }

    private async Task AddContact()
    {
        if (NewContactDisplayName == string.Empty)
            NewContactDisplayName = NewContactEmail;

        if (ContactActions == null)
            return;

        await ContactActions.AddContact(NewContactEmail, NewContactDisplayName);
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

        if (ContactActions == null)
            return;

        await ContactActions.RemoveContact(email);
        foreach (ContactGroup group in ListData.ContactGroups)
        {
            group.Contacts.Remove(SelectedContact);
        }
    }

    private async Task BlockContact()
    {
        if (SelectedContact == null || ContactActions == null)
            return;

        await ContactActions.BlockContact(SelectedContact.Email);
        SelectedContact.Blocked = true;
    }

    private async Task UnblockContact()
    {
        if (SelectedContact == null || ContactActions == null)
            return;

        await ContactActions.UnblockContact(SelectedContact.Email);
        SelectedContact.Blocked = false;
    }

    /// <summary>
    /// Creates a new conversation object with the selected contact and opens a new conversation window.
    /// If a conversation with the selected contact already exists however, simply opens its window.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if the contact is not found in the contact list.</exception>
    private async Task Chat()
    {
        if (SelectedContact == null || ContactActions?.Server?.Incoming == null)
            return;

        // Unbold contact name
        SelectedContact.NewMessages = false;

        Conversation? conversation = Conversations.LastOrDefault(conv => conv.Contact == SelectedContact);
        MSNP.Models.Contact contact = ContactActions.ContactList.FirstOrDefault(c => c.Email == SelectedContact.Email) ??
                                      throw new ContactException("Could not find the selected contact");
        
        if (conversation == null)
        {
            conversation = new Conversation(SelectedContact, User, Database)
            {
                ContactActions = ContactActions
            };

            conversation.DisplayPictureUpdated += Conversation_DisplayPictureUpdated;
            if (ContactActions?.Server?.Incoming != null)
                ContactActions.Server.Incoming.SwitchboardChanged += conversation.NotificationServer_SwitchboardChanged;

            Conversations.Add(conversation);
        }

        conversation.OpenWindow();
        if (conversation.Messaging.Server == null || !conversation.Messaging.Server.Connected)
        {
            conversation.Messaging.Server = await ContactActions!.Server.SendXFR(contact);
            conversation.Messaging.StartIncoming();
            conversation.SubscribeToEvents();
        }

        try
        {
            if (contact.DisplayPictureHash != SelectedContact.DisplayPictureHash && conversation.Messaging.Server.ContactInSession)
                _ = conversation.Messaging.IncomingMessaging?.DisplayPictureTransfer?.GetDisplayPicture();
        }
        catch (OperationCanceledException)
        {
            // Start receiving incoming commands again
            _ = conversation.Messaging.Server.ReceiveIncomingAsync();
            return;
        }
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
            SettingsWindow = new SettingsWindow
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
        if (ContactActions == null || ContactActions.Server == null)
            return;

        // Will also invoke the disconnection event
        await ContactActions.Server.DisconnectAsync();
    }

    /// <summary>
    /// Subscribes to notification server events.
    /// </summary>
    public void SubscribeToEvents()
    {
        ContactActions!.Server!.Disconnected += NotificationServer_Disconnected;
        ContactActions.Server.Incoming!.SwitchboardChanged += NotificationServer_SwitchboardChanged;
    }

    /// <summary>
    /// Unsubscribes from notification server events.
    /// </summary>
    public void UnsubscribeFromEvents()
    {
        ContactActions!.Server!.Disconnected -= NotificationServer_Disconnected;
        ContactActions.Server.Incoming!.SwitchboardChanged -= NotificationServer_SwitchboardChanged;
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
    private void NotificationServer_SwitchboardChanged(object? sender, SwitchboardEventArgs e)
    {
        if (ContactGroups == null || e.Switchboard == null)
            return;

        Conversation? conversation = Conversations.LastOrDefault(conv => conv.Contact.Email == e.Switchboard?.Contact?.Email);
        if (conversation == null)
        {
            Contact contact = new Contact
            {
                Email = e.Switchboard.Contact!.Email,
                DisplayName = e.Switchboard.Contact.DisplayName,
                Presence = PresenceStatus.GetFullName(e.Switchboard.Contact.Presence)
            };

            foreach (ContactGroup group in ContactGroups)
            {
                contact = group.Contacts.FirstOrDefault(c => c?.Email == e.Switchboard.Contact.Email) ?? contact;
            }

            conversation = new Conversation(contact, User, Database!)
            {
                Messaging =
                {
                    Server = e.Switchboard
                },
                ContactActions = ContactActions
            };

            if (ContactActions?.Server?.Incoming != null)
                ContactActions.Server.Incoming.SwitchboardChanged += conversation.NotificationServer_SwitchboardChanged;

            conversation.Messaging.StartIncoming();
            conversation.SubscribeToEvents();
            Conversations.Add(conversation);
        }
    }

    /// <summary>
    /// Resets data, closes chat windows and returns to login page when disconnected from the server.
    /// </summary>
    /// <exception cref="ConnectionException">Thrown if the disconnection wasn't requested.</exception>
    private async void NotificationServer_Disconnected(object? sender, MSNP.Models.DisconnectedEventArgs e)
    {
        // Close every conversation
        foreach (Conversation conversation in Conversations)
        {
            await conversation.Disconnect();
            if (conversation.ContactActions?.Server?.Incoming != null)
                conversation.ContactActions.Server.Incoming.SwitchboardChanged -= conversation.NotificationServer_SwitchboardChanged;
            
            conversation.DisplayPictureUpdated -= Conversation_DisplayPictureUpdated;
            conversation.CloseWindow();
        }

        UnsubscribeFromEvents();
        Conversations = [];
        ContactActions = null;
        UserProfile = null;
        SelectedContact = null;
        ListData = new ContactListData();
        Disconnected?.Invoke(this, new Models.DisconnectedEventArgs
        {
            RedirectedByTheServer = e.RedirectedByTheServer,
            NewServerHost = e.NewServerHost,
            NewServerPort = e.NewServerPort
        });

        if (!e.Requested)
            NotificationHandler?.ShowError("Lost connection to the server");

        if (e.RedirectedByTheServer)
            NotificationHandler?.ShowError("Logging in to a new server as requested by the old one...");
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
                Contact? contact = group.Contacts.FirstOrDefault(contact => contact?.Email == conversation.Contact.Email);
                if (contact != null)
                    contact.DisplayPicture = conversation.Contact.DisplayPicture;
            }
        }
    }
}
