using AvaMSN.MSNP;
using ReactiveUI;
using System.Reactive;
using AvaMSN.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace AvaMSN.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string email = string.Empty;
    private string password = string.Empty;

    private bool rememberMe;
    private bool rememberPassword;

    public NotificationServer NotificationServer { get; set; }

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

        SettingsManager manager = new SettingsManager();
        NotificationServer = new NotificationServer(SettingsManager.Settings.Server)
        {
            Port = 1863
        };

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

            Email = user.UserEmail;
            Password = user.BinarySecret;

            if (user.UserEmail != "")
                RememberMe = true;

            if (user.BinarySecret != "")
                RememberPassword = true;

            NotificationServer.ContactList.Profile.PersonalMessage = user.PersonalMessage;
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

        NotificationServer.ContactList.Profile.PersonalMessage = user.PersonalMessage;
    }

    public async Task Login()
    {
        NotificationServer.ContactList.Profile.Presence = SelectedStatus.ShortName;
        NotificationServer.ContactList.Profile.Email = Email;

        User user = Users?.FirstOrDefault(user => user.UserEmail == Email) ?? new User();

        await NotificationServer.SendVersion();

        if (user.BinarySecret == ""
            || user.TicketToken == ""
            || user.Ticket == "")
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

        await NotificationServer.SendContactList();
        await NotificationServer.SendUUX();
        await NotificationServer.SendCHG();

        LoggedIn?.Invoke(this, EventArgs.Empty);

        if (RememberMe)
            user.UserEmail = Email;

        if (RememberPassword)
        {
            user.Ticket = NotificationServer.SSO.Ticket;
            user.BinarySecret = NotificationServer.SSO.BinarySecret;
            user.TicketToken = NotificationServer.ContactList.TicketToken;
        }

        if (RememberMe)
            Database?.SaveUser(user);

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
        Users?.Remove(user);
    }
}
