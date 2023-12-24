using AvaMSN.MSNP;
using ReactiveUI;
using System.Reactive;
using AvaMSN.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.IO;
using AvaMSN.Views;

namespace AvaMSN.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string email = string.Empty;
    private string password = string.Empty;

    private bool rememberMe;
    private bool rememberPassword;

    public NotificationServer? NotificationServer { get; set; }

    public bool RememberMe
    {
        get => rememberMe;
        set => this.RaiseAndSetIfChanged(ref rememberMe, value);
    }

    public bool RememberPassword
    {
        get => rememberPassword;
        set
        {
            if (!RememberMe)
                RememberMe = value;

            this.RaiseAndSetIfChanged(ref rememberPassword, value);
        }
    }

    public string Email
    {
        get => email;
        set => this.RaiseAndSetIfChanged(ref email, value);
    }

    public string Password
    {
        get => password;
        set => this.RaiseAndSetIfChanged(ref password, value);
    }

    public Models.Profile Profile { get; set; } = new Models.Profile();

    public Presence[] Statuses => ContactListData.Statuses;
    public Presence SelectedStatus { get; set; }

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<Unit, Unit> ForgetMeCommand { get; }
    public ReactiveCommand<string, Unit> ChangeUserCommand { get; }

    public Database Database { get; set; } = new Database();
    public ObservableCollection<User>? Users { get; set; }

    public event EventHandler? LoggedIn;

    public LoginViewModel()
    {
        LoginCommand = ReactiveCommand.CreateFromTask(Login);
        ForgetMeCommand = ReactiveCommand.Create(ForgetMe);
        ChangeUserCommand = ReactiveCommand.Create<string>(ChangeUser);

        SelectedStatus = Statuses[0];

        Profile.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/default-display-picture.png")));
        GetUsers();
    }

    /// <summary>
    /// Loads saved user accounts and display options.
    /// </summary>
    public void GetUsers()
    {
        Users = new ObservableCollection<User>(Database.GetUsers())
        {
            new User
            {
                UserEmail = "New user"
            },
            new User
            {
                UserEmail = "Options"
            }
        };

        if (Users.Count > 1)
        {
            User user = Users[0];
            ChangeUser(user.UserEmail);
        }
    }

    /// <summary>
    /// Displays selected user or runs selected option.
    /// </summary>
    /// <param name="option">Account email or option.</param>
    public void ChangeUser(string option)
    {
        switch (option)
        {
            case "New user":
                Email = string.Empty;
                Password = string.Empty;

                RememberMe = false;
                RememberPassword = false;

                Profile.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/default-display-picture.png")));
                return;

            case "Options":
                OpenOptions();
                return;
        }

        User? user = Users?.FirstOrDefault(user => user.UserEmail == option);

        if (user == null)
            return;

        Email = user.UserEmail;
        Password = user.BinarySecret;

        if (user.UserEmail != "")
            RememberMe = true;

        if (user.BinarySecret != "")
            RememberPassword = true;

        Profile.PersonalMessage = user.PersonalMessage;
        LoadDisplayPicture(user.UserEmail);
    }

    /// <summary>
    /// Retrieves and sets a user display picture from the database. If none are found the default picture is set.
    /// </summary>
    /// <param name="email">User email.</param>
    public void LoadDisplayPicture(string email)
    {
        DisplayPicture? picture = Database?.GetUserDisplayPicture(email);
        if (picture != null)
        {
            if (picture.PictureData.Length > 0)
            {
                using MemoryStream pictureStream = new MemoryStream(picture.PictureData);
                Profile.DisplayPicture = new Bitmap(pictureStream);
                pictureStream.Position = 0;

                Profile.DisplayPictureData = pictureStream.ToArray();
            }
        }
        else
        {
            Profile.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/default-display-picture.png")));
        }
    }

    /// <summary>
    /// Initiates a new notification server connection, does every login step and saves user if the remember options are enabled.
    /// </summary>
    /// <returns></returns>
    public async Task Login()
    {
        LoadDisplayPicture(Email);

        NotificationServer = new NotificationServer(SettingsManager.Settings.Server)
        {
            Port = 1863
        };

        NotificationServer.Profile.Presence = SelectedStatus.ShortName;
        NotificationServer.Profile.Email = Email;
        NotificationServer.Profile.PersonalMessage = Profile.PersonalMessage;
        NotificationServer.Profile.DisplayPicture = Profile.DisplayPictureData ?? NotificationServer.Profile.DisplayPicture;

        await NotificationServer.SendVersion();

        // Use token and binary secret if available
        User user = Users?.FirstOrDefault(user => user.UserEmail == Email) ?? new User();

        if (user.BinarySecret != "" & user.TicketToken != "" & user.Ticket != "")
        {
            NotificationServer.SSO.Ticket = user.Ticket;
            NotificationServer.SSO.BinarySecret = user.BinarySecret;
            NotificationServer.SSO.TicketToken = user.TicketToken;

            await NotificationServer.AuthenticateWithToken();
        }

        else
        {
            await NotificationServer.Authenticate(Password);
        }

        await NotificationServer.GetContactList();

        // Presence and personal message
        await NotificationServer.SendUUX();
        NotificationServer.CreateMSNObject();
        await NotificationServer.SendCHG();

        // Start regularly pinging after presence is set
        _ = NotificationServer.Ping();

        // Navigate to contact list
        LoggedIn?.Invoke(this, EventArgs.Empty);

        if (RememberPassword)
        {
            user.Ticket = NotificationServer.SSO.Ticket;
            user.BinarySecret = NotificationServer.SSO.BinarySecret;
            user.TicketToken = NotificationServer.ContactList.TicketToken;
        }

        if (RememberMe)
        {
            user.UserEmail = Email;
            Database?.SaveUser(user);
        }

        Email = string.Empty;
        Password = string.Empty;
    }

    /// <summary>
    /// Empties fields and removes a user account from user list and database.
    /// </summary>
    public void ForgetMe()
    {
        User? user = Users?.FirstOrDefault(user => user.UserEmail == Email);

        if (user == null)
            return;

        Email = string.Empty;
        Password = string.Empty;

        Users?.Remove(user);
        Database?.DeleteUser(user);
        Profile.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/default-display-picture.png")));
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
}
