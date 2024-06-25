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
using AvaMSN.MSNP.Exceptions;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AvaMSN.Utils;

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
        Profile.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN.Shared/Assets/default-display-picture.png")));
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

                Profile.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN.Shared/Assets/default-display-picture.png")));
                return;

            case "Options":
                OpenOptions();
                return;
        }

        User? user = Users?.FirstOrDefault(user => user.UserEmail == option);
        if (user == null)
            return;

        Email = user.UserEmail;
        Password = Encoding.UTF8.GetString(user.BinarySecret);

        if (Email != "")
            RememberMe = true;

        if (Password != "")
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
            Profile.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN.Shared/Assets/default-display-picture.png")));
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
        User user = Users?.LastOrDefault(user => user.UserEmail == Email) ?? new User();
        if (user.BinarySecret.Length > 0 & user.TicketToken.Length > 0 & user.Ticket.Length > 0)
        {
            Keys? keys = Database.GetUserKeys(user);

            // Decrypt and read user data
            if (keys != null)
            {
                using (MemoryStream ticketStream = new MemoryStream(user.Ticket))
                {
                    using (Aes aes = Aes.Create())
                    {
                        using CryptoStream cryptoStream = new CryptoStream(ticketStream, aes.CreateDecryptor(keys.Key1, keys.IV1), CryptoStreamMode.Read);
                        using StreamReader encryptReader = new StreamReader(cryptoStream);
                        NotificationServer.SSO.Ticket = await encryptReader.ReadToEndAsync();
                        NotificationServer.SSO.Ticket = Regex.Replace(NotificationServer.SSO.Ticket, @"\t|\r|\n", "");
                    }
                }

                using (MemoryStream ticketTokenStream = new MemoryStream(user.TicketToken))
                {
                    using (Aes aes = Aes.Create())
                    {
                        using CryptoStream cryptoStream = new CryptoStream(ticketTokenStream, aes.CreateDecryptor(keys.Key3, keys.IV3), CryptoStreamMode.Read);
                        using StreamReader encryptReader = new StreamReader(cryptoStream);
                        NotificationServer.SSO.TicketToken = await encryptReader.ReadToEndAsync();
                        NotificationServer.SSO.TicketToken = Regex.Replace(NotificationServer.SSO.TicketToken, @"\t|\r|\n", "");
                    }
                }

                using (MemoryStream binarySecretStream = new MemoryStream(user.BinarySecret))
                {
                    using (Aes aes = Aes.Create())
                    {
                        using CryptoStream cryptoStream = new CryptoStream(binarySecretStream, aes.CreateDecryptor(keys.Key4, keys.IV4), CryptoStreamMode.Read);
                        using StreamReader encryptReader = new StreamReader(cryptoStream);
                        NotificationServer.SSO.BinarySecret = await encryptReader.ReadToEndAsync();
                        NotificationServer.SSO.BinarySecret = Regex.Replace(NotificationServer.SSO.BinarySecret, @"\t|\r|\n", "");
                    }
                }
            }

            try
            {
                await NotificationServer.AuthenticateWithToken();
            }
            catch (AuthException)
            {
                // Create new connection and try to login again with the password
                await NotificationServer.DisconnectAsync();
                NotificationServer = new NotificationServer(SettingsManager.Settings.Server)
                {
                    Port = 1863
                };

                NotificationServer.Profile.Presence = SelectedStatus.ShortName;
                NotificationServer.Profile.Email = Email;
                NotificationServer.Profile.PersonalMessage = Profile.PersonalMessage;
                NotificationServer.Profile.DisplayPicture = Profile.DisplayPictureData ?? NotificationServer.Profile.DisplayPicture;
                await NotificationServer.SendVersion();

                string password = string.Empty;
                using (MemoryStream passwordStream = new MemoryStream(user.Password))
                {
                    using (Aes aes = Aes.Create())
                    {
                        if (keys != null)
                        {
                            using CryptoStream cryptoStream = new CryptoStream(passwordStream, aes.CreateDecryptor(keys.Key2, keys.IV2), CryptoStreamMode.Read);
                            using StreamReader encryptReader = new StreamReader(cryptoStream);
                            password = await encryptReader.ReadToEndAsync();
                            password = Regex.Replace(password, @"\t|\r|\n", "");
                        }
                    }
                }

                await NotificationServer.Authenticate(password);
                SaveUserData(user, password);
            }
        }

        else
        {
            await NotificationServer.Authenticate(Password);
            SaveUserData(user, Password);
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

        Email = string.Empty;
        Password = string.Empty;
    }

    /// <summary>
    /// Saves user data to the database.
    /// </summary>
    /// <param name="user">User to save.</param>
    /// <param name="password">User password to save.</param>
    private void SaveUserData(User user, string password)
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
                    encryptWriter.WriteLine(NotificationServer?.SSO.Ticket);

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
                    encryptWriter.WriteLine(password);

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
                    encryptWriter.WriteLine(NotificationServer?.ContactService.TicketToken);

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
                    encryptWriter.WriteLine(NotificationServer?.SSO.BinarySecret);

                    keys.Key4 = aes.Key;
                    keys.IV4 = aes.IV;
                }

                user.BinarySecret = binarySecretStream.ToArray();
            }

            keys.UserEmail = user.UserEmail;
            Database?.SaveKeys(keys);
        }

        if (RememberMe)
            Database?.SaveUser(user);
    }

    /// <summary>
    /// Empties fields and removes a user account from user list and database.
    /// </summary>
    public void ForgetMe()
    {
        User? user = Users?.LastOrDefault(user => user.UserEmail == Email);

        if (user == null)
            return;

        Email = string.Empty;
        Password = string.Empty;

        Users?.Remove(user);
        Database?.DeleteUser(user);
        Profile.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN.Shared/Assets/default-display-picture.png")));
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
