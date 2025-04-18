﻿using ReactiveUI;
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
using AvaMSN.MSNP.Exceptions;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AvaMSN.MSNP.NotificationServer;
using AvaMSN.MSNP.NotificationServer.Contacts;
using AvaMSN.MSNP.NotificationServer.UserProfile;
using AvaMSN.MSNP.NotificationServer.Authentication;
using AvaMSN.Utils;

namespace AvaMSN.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private bool rememberMe;
    private bool rememberPassword;
    private bool signInButtonEnabled = true;
    private string email = string.Empty;
    private string password = string.Empty;
    private string binarySecret = string.Empty;
    private string ticketToken = string.Empty;
    private string ticket = string.Empty;

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

    public bool SignInButtonEnabled
    {
        get => signInButtonEnabled;
        set => this.RaiseAndSetIfChanged(ref signInButtonEnabled, value);
    }

    public bool ComingFromAPreviousRedirection { get; set; }
    public string NewServer { get; set; } = SettingsManager.Settings.MainServer;
    public int NewPort { get; set; } = 1863;

    public Database Database => new Database();
    public User User { get; set; } = new User();
    public Presence SelectedStatus { get; set; }
    public Presence[] Statuses => ContactListData.Statuses;
    public ObservableCollection<StoredUser>? Users { get; set; }

    public ReactiveCommand<Unit, Unit> SignInCommand { get; }
    public ReactiveCommand<Unit, Unit> ForgetMeCommand { get; }
    public ReactiveCommand<string, Unit> ChangeUserCommand { get; }

    public event EventHandler<LoggedInEventArgs>? LoggedIn;

    public LoginViewModel()
    {
        SignInCommand = ReactiveCommand.CreateFromTask(SignIn);
        ForgetMeCommand = ReactiveCommand.Create(ForgetMe);
        ChangeUserCommand = ReactiveCommand.Create<string>(ChangeUser);

        SelectedStatus = Statuses[0];
        User.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN.Shared/Assets/default-display-picture.png")));
        GetUsers();
    }

    /// <summary>
    /// Loads saved user accounts and display options.
    /// </summary>
    public void GetUsers()
    {
        Users = new ObservableCollection<StoredUser>(Database.GetUsers())
        {
            new StoredUser
            {
                UserEmail = "New user"
            },
            new StoredUser
            {
                UserEmail = "Options"
            }
        };

        if (Users.Count > 1)
        {
            if (Email != "")
                ChangeUser(Email);
            else
                ChangeUser(Users[0].UserEmail);
        }
    }

    /// <summary>
    /// Displays selected user or runs selected option.
    /// </summary>
    /// <param name="option">Account email or option.</param>
    private void ChangeUser(string option)
    {
        switch (option)
        {
            case "New user":
                Email = string.Empty;
                Password = string.Empty;

                RememberMe = false;
                RememberPassword = false;

                User.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN.Shared/Assets/default-display-picture.png")));
                return;

            case "Options":
                OpenOptions();
                return;
        }

        StoredUser? user = (Users?.FirstOrDefault(user => user.UserEmail == option));
        if (user == null)
            return;

        Email = user.UserEmail;
        Password = Encoding.UTF8.GetString(user.BinarySecret);

        if (Email != "")
            RememberMe = true;

        if (Password != "")
            RememberPassword = true;

        User.PersonalMessage = user.PersonalMessage;
        LoadDisplayPicture(user.UserEmail);
    }

    /// <summary>
    /// Retrieves and sets a user display picture from the database. If none are found the default picture is set.
    /// </summary>
    /// <param name="userEmail">User userEmail.</param>
    private void LoadDisplayPicture(string userEmail)
    {
        DisplayPicture? picture = Database.GetUserDisplayPicture(userEmail);
        if (picture is { PictureData.Length: > 0 })
        {
            using MemoryStream pictureStream = new MemoryStream(picture.PictureData);
            User.DisplayPicture = new Bitmap(pictureStream);
            User.DisplayPictureData = picture.PictureData;
        }
        else
        {
            User.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN.Shared/Assets/default-display-picture.png")));
        }
    }

    /// <summary>
    /// Initiates a new notification server connection, does every login step and saves user if the remember options are enabled.
    /// </summary>
    /// <returns></returns>
    public async Task SignIn()
    {
        LoadDisplayPicture(Email);
        Authentication authentication = await CreateConnection();

        // Use ticket and binary secret if they're available and the password box wasn't modified
        StoredUser user = Users?.LastOrDefault(user => user.UserEmail == Email) ?? new StoredUser();
        if (user.BinarySecret.Length > 0
            && user.TicketToken.Length > 0
            && user.Ticket.Length > 0
            && Password == Encoding.UTF8.GetString(user.BinarySecret))
        {
            await GetUserData(user, authentication);

            try
            {
                await authentication.AuthenticateWithTicket();
            }
            catch (RedirectedByTheServerException e)
            {
                ComingFromAPreviousRedirection = false;
                await authentication.Server.DisconnectAsync();
                authentication = await CreateConnection(e.Server, e.Port);
                await GetUserData(user, authentication);

                try
                {
                    await authentication.AuthenticateWithTicket();
                }
                catch (MsnpServerAuthException)
                {
                    // Create new connection and use password
                    await authentication.Server.DisconnectAsync();
                    authentication = await CreateConnection(e.Server, e.Port);

                    string userPassword = string.Empty;
                    using (MemoryStream passwordStream = new MemoryStream(user.Password))
                    {
                        using Aes aes = Aes.Create();
                        Keys? keys = Database.GetUserKeys(user);
                        if (keys != null)
                        {
                            await using CryptoStream cryptoStream = new CryptoStream(passwordStream, aes.CreateDecryptor(keys.Key2, keys.IV2), CryptoStreamMode.Read);
                            using StreamReader encryptReader = new StreamReader(cryptoStream);
                            userPassword = await encryptReader.ReadToEndAsync();
                            userPassword = Regex.Replace(userPassword, @"\t|\r|\n", "");
                        }
                    }

                    await authentication.Authenticate(userPassword);
                    ticket = authentication.SSO.Ticket;
                    ticketToken = authentication.SSO.TicketToken;
                    binarySecret = authentication.SSO.BinarySecret;
                    SaveUserData(user, userPassword);
                }
            }
            catch (MsnpServerAuthException)
            {
                // Create new connection and use password
                await authentication.Server.DisconnectAsync();
                authentication = await CreateConnection();

                string userPassword = string.Empty;
                using (MemoryStream passwordStream = new MemoryStream(user.Password))
                {
                    using Aes aes = Aes.Create();
                    Keys? keys = Database.GetUserKeys(user);
                    if (keys != null)
                    {
                        await using CryptoStream cryptoStream = new CryptoStream(passwordStream, aes.CreateDecryptor(keys.Key2, keys.IV2), CryptoStreamMode.Read);
                        using StreamReader encryptReader = new StreamReader(cryptoStream);
                        userPassword = await encryptReader.ReadToEndAsync();
                        userPassword = Regex.Replace(userPassword, @"\t|\r|\n", "");
                    }
                }

                await authentication.Authenticate(userPassword);
                ticket = authentication.SSO.Ticket;
                ticketToken = authentication.SSO.TicketToken;
                binarySecret = authentication.SSO.BinarySecret;
                SaveUserData(user, userPassword);
            }
        }
        else
        {
            try
            {
                await authentication.Authenticate(Password);
            }
            catch (RedirectedByTheServerException e)
            {
                ComingFromAPreviousRedirection = false;
                await authentication.Server.DisconnectAsync();
                authentication = await CreateConnection(e.Server, e.Port);
                await authentication.Authenticate(Password);
            }

            ticket = authentication.SSO.Ticket;
            ticketToken = authentication.SSO.TicketToken;
            binarySecret = authentication.SSO.BinarySecret;
            SaveUserData(user, Password);
        }

        ContactActions contactActions = new ContactActions
        {
            Server = authentication.Server
        };
        
        UserProfile userProfile = new UserProfile
        {
            Server = authentication.Server
        };

        // Contacts, presence and personal message
        await contactActions.SendContactList();
        await userProfile.SendUUX();
        userProfile.CreateMSNObject();
        await userProfile.SendCHG();

        // Start regularly pinging after presence is set
        _ = authentication.Server.StartPinging();

        // Navigate to contact list
        LoggedIn?.Invoke(this, new LoggedInEventArgs
        {
            ContactActions = contactActions,
            UserProfile = userProfile
        });

        Password = string.Empty;
        ComingFromAPreviousRedirection = false;
        NewServer = SettingsManager.Settings.MainServer;
        NewPort = 1863;
    }

    /// <summary>
    /// Connects to the server and returns an authentication object.
    /// </summary>
    /// <param name="host">Server URL or IP.</param>
    /// <param name="port">Server port.</param>
    /// <returns></returns>
    private async Task<Authentication> CreateConnection(string? host = null, int port = 1863)
    {
        if (ComingFromAPreviousRedirection)
        {
            host = NewServer;
            port = NewPort;
        }

        NotificationServer notificationServer = new NotificationServer
        {
            Host = host ?? SettingsManager.Settings.MainServer,
            Port = port,
            RstUrl = SettingsManager.Settings.RstUrl,
            SharingServiceUrl = SettingsManager.Settings.SharingServiceUrl,
            AddressBookUrl = SettingsManager.Settings.AddressBookUrl,
            User =
            {
                Presence = SelectedStatus.ShortName,
                Email = Email,
                PersonalMessage = User.PersonalMessage
            }
        };

        notificationServer.User.DisplayPicture = User.DisplayPictureData ?? notificationServer.User.DisplayPicture;
        await notificationServer.Connect();

        Authentication authentication = new Authentication(notificationServer);
        await authentication.SendVER();
        await authentication.SendCVR();

        return authentication;
    }

    /// <summary>
    /// Gets user data from the database and sets it in an authentication object.
    /// </summary>
    /// <param name="user">User to get data from.</param>
    /// <param name="authentication">Authentication object that will get the data.</param>
    /// <returns></returns>
    private async Task GetUserData(StoredUser user, Authentication authentication)
    {
        if (user.BinarySecret.Length > 0 & user.TicketToken.Length > 0 & user.Ticket.Length > 0)
        {
            Keys? keys = Database.GetUserKeys(user);

            // Decrypt and read user data
            if (keys != null)
            {
                using (MemoryStream ticketStream = new MemoryStream(user.Ticket))
                {
                    using Aes aes = Aes.Create();
                    await using CryptoStream cryptoStream = new CryptoStream(ticketStream, aes.CreateDecryptor(keys.Key1, keys.IV1), CryptoStreamMode.Read);
                    using StreamReader encryptReader = new StreamReader(cryptoStream);
                    authentication.SSO.Ticket = await encryptReader.ReadToEndAsync();
                    authentication.SSO.Ticket = Regex.Replace(authentication.SSO.Ticket, @"\t|\r|\n", "");
                }

                using (MemoryStream ticketTokenStream = new MemoryStream(user.TicketToken))
                {
                    using Aes aes = Aes.Create();
                    await using CryptoStream cryptoStream = new CryptoStream(ticketTokenStream, aes.CreateDecryptor(keys.Key3, keys.IV3), CryptoStreamMode.Read);
                    using StreamReader encryptReader = new StreamReader(cryptoStream);
                    authentication.SSO.TicketToken = await encryptReader.ReadToEndAsync();
                    authentication.SSO.TicketToken = Regex.Replace(authentication.SSO.TicketToken, @"\t|\r|\n", "");
                }

                using (MemoryStream binarySecretStream = new MemoryStream(user.BinarySecret))
                {
                    using Aes aes = Aes.Create();
                    await using CryptoStream cryptoStream = new CryptoStream(binarySecretStream, aes.CreateDecryptor(keys.Key4, keys.IV4), CryptoStreamMode.Read);
                    using StreamReader encryptReader = new StreamReader(cryptoStream);
                    authentication.SSO.BinarySecret = await encryptReader.ReadToEndAsync();
                    authentication.SSO.BinarySecret = Regex.Replace(authentication.SSO.BinarySecret, @"\t|\r|\n", "");
                }
            }
        }
    }

    /// <summary>
    /// Saves user data to the database.
    /// </summary>
    /// <param name="user">User to save.</param>
    /// <param name="userPassword">User userPassword to save.</param>
    private void SaveUserData(StoredUser user, string userPassword)
    {
        user.UserEmail = Email;

        if (RememberPassword)
        {
            Keys keys = new Keys();
            using (MemoryStream ticketStream = new MemoryStream())
            {
                using (Aes aes = Aes.Create())
                {
                    using CryptoStream cryptoStream = new CryptoStream(ticketStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    using StreamWriter encryptWriter = new StreamWriter(cryptoStream);
                    encryptWriter.WriteLine(ticket);

                    keys.Key1 = aes.Key;
                    keys.IV1 = aes.IV;
                }

                user.Ticket = ticketStream.ToArray();
            }

            using (MemoryStream passwordStream = new MemoryStream())
            {
                using (Aes aes = Aes.Create())
                {
                    using CryptoStream cryptoStream = new CryptoStream(passwordStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    using StreamWriter encryptWriter = new StreamWriter(cryptoStream);
                    encryptWriter.WriteLine(userPassword);

                    keys.Key2 = aes.Key;
                    keys.IV2 = aes.IV;
                }

                user.Password = passwordStream.ToArray();
            }

            using (MemoryStream ticketTokenStream = new MemoryStream())
            {
                using (Aes aes = Aes.Create())
                {
                    using CryptoStream cryptoStream = new CryptoStream(ticketTokenStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    using StreamWriter encryptWriter = new StreamWriter(cryptoStream);
                    encryptWriter.WriteLine(ticketToken);

                    keys.Key3 = aes.Key;
                    keys.IV3 = aes.IV;
                }

                user.TicketToken = ticketTokenStream.ToArray();
            }

            using (MemoryStream binarySecretStream = new MemoryStream())
            {
                using (Aes aes = Aes.Create())
                {
                    using CryptoStream cryptoStream = new CryptoStream(binarySecretStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    using StreamWriter encryptWriter = new StreamWriter(cryptoStream);
                    encryptWriter.WriteLine(binarySecret);

                    keys.Key4 = aes.Key;
                    keys.IV4 = aes.IV;
                }

                user.BinarySecret = binarySecretStream.ToArray();
            }

            keys.UserEmail = user.UserEmail;
            Database.SaveKeys(keys);
        }

        if (RememberMe)
            Database.SaveUser(user);
    }

    /// <summary>
    /// Empties fields and removes a user account from user list and database.
    /// </summary>
    private void ForgetMe()
    {
        StoredUser? user = Users?.LastOrDefault(user => user.UserEmail == Email);

        if (user == null)
            return;

        Email = string.Empty;
        Password = string.Empty;

        Users?.Remove(user);
        Database.DeleteUser(user);
        User.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN.Shared/Assets/default-display-picture.png")));
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
}
