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
    public event EventHandler? OptionsButtonPressed;

    public LoginViewModel()
    {
        LoginCommand = ReactiveCommand.CreateFromTask(Login);
        ForgetMeCommand = ReactiveCommand.Create(ForgetMe);
        ChangeUserCommand = ReactiveCommand.Create<string>(ChangeUser);

        SelectedStatus = Statuses[0];

        Profile.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/default-display-picture.png")));
        GetUsers();
    }

    public void GetUsers()
    {
        Users = new ObservableCollection<User>(Database.GetUsers())
        {
            new User
            {
                UserEmail = "New User"
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

    public void ChangeUser(string option)
    {
        switch (option)
        {
            case "New User":
                Email = string.Empty;
                Password = string.Empty;

                RememberMe = false;
                RememberPassword = false;

                Profile.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/default-display-picture.png")));
                return;

            case "Options":
                OptionsButtonPressed?.Invoke(this, EventArgs.Empty);
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

        DisplayPicture? picture = Database?.GetUserDisplayPicture(user.UserEmail);
        if (picture != null && picture.PictureData.Length > 0)
        {
            using MemoryStream pictureStream = new MemoryStream(picture.PictureData);
            Profile.DisplayPicture = new Bitmap(pictureStream);
            pictureStream.Position = 0;

            Profile.DisplayPictureData = pictureStream.ToArray();
        }
    }

    public async Task Login()
    {
        NotificationServer = new NotificationServer(SettingsManager.Settings.Server)
        {
            Port = 1863
        };

        NotificationServer.Profile.Presence = SelectedStatus.ShortName;
        NotificationServer.Profile.Email = Email;
        NotificationServer.Profile.PersonalMessage = Profile.PersonalMessage;
        NotificationServer.Profile.DisplayPicture = Profile.DisplayPictureData ?? NotificationServer.Profile.DisplayPicture;

        await NotificationServer.SendVersion();

        User user = Users?.FirstOrDefault(user => user.UserEmail == Email) ?? new User();

        if (user.BinarySecret == "" || user.TicketToken == "" || user.Ticket == "")
        {
            await NotificationServer.Authenticate(Password);
        }

        else
        {
            NotificationServer.SSO.Ticket = user.Ticket;
            NotificationServer.SSO.BinarySecret = user.BinarySecret;
            NotificationServer.SSO.TicketToken = user.TicketToken;

            await NotificationServer.AuthenticateWithToken();
        }

        await NotificationServer.GetContactList();

        await NotificationServer.SendUUX();
        NotificationServer.GenerateMSNObject();
        await NotificationServer.SendCHG();

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

    public void ForgetMe()
    {
        User? user = Users?.FirstOrDefault(user => user.UserEmail == Email);

        if (user == null)
            return;

        Email = string.Empty;
        Password = string.Empty;

        Database?.DeleteUser(user);
        Database?.DeleteDisplayPictures(user.UserEmail);
        Users?.Remove(user);
        Profile.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/default-display-picture.png")));
    }
}
