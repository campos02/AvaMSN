using AvaMSN.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AvaMSN;

/// <summary>
/// Provides functions for saving and retrieving from the database.
/// </summary>
public class Database
{
    private readonly SQLiteConnection connection;

    public static string FileName => "AvaMSN.db";
    public static string FileDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AvaMSN");
    public static string FilePath => Path.Combine(FileDirectory, FileName);

    public Database()
    {
        if (!Directory.Exists(FileDirectory))
            Directory.CreateDirectory(FileDirectory);

        connection = new SQLiteConnection(FilePath);
        connection.CreateTable<User>();
        connection.CreateTable<Message>();
        connection.CreateTable<DisplayPicture>();
    }

    /// <summary>
    /// Gets a list of all user accounts saved.
    /// </summary>
    /// <returns></returns>
    public List<User> GetUsers()
    {
        return connection.Table<User>().ToList();
    }

    /// <summary>
    /// Saves a user account.
    /// </summary>
    /// <param name="user">User account to be saved.</param>
    /// <returns></returns>
    public int SaveUser(User user)
    {
        if (user.ID != 0)
            return connection.Update(user);

        else
            return connection.Insert(user);
    }

    /// <summary>
    /// Deletes a user account.
    /// </summary>
    /// <param name="user">User account to be deleted.</param>
    /// <returns></returns>
    public int DeleteUser(User user)
    {
        return connection.Delete(user);
    }

    /// <summary>
    /// Returns all messages from all contacts.
    /// </summary>
    /// <returns></returns>
    public List<Message> GetMessages()
    {
        return connection.Table<Message>().ToList();
    }

    /// <summary>
    /// Returns all messages between two contacts.
    /// </summary>
    /// <param name="contact1"></param>
    /// <param name="contact2"></param>
    /// <returns></returns>
    public List<Message> GetMessages(string contact1, string contact2)
    {
        return connection.Table<Message>().Where(message => (message.Sender == contact1 || message.Recipient == contact1)
                                                 && (message.Recipient == contact2 || message.Sender == contact2)).ToList();
    }

    /// <summary>
    /// Saves a new message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public int SaveMessage(Message message)
    {
        return connection.Insert(message);
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public int DeleteMessage(Message message)
    {
        return connection.Delete(message);
    }

    /// <summary>
    /// Deletes all messages between two contacts.
    /// </summary>
    /// <param name="contact1"></param>
    /// <param name="contact2"></param>
    public void DeleteMessages(string contact1, string contact2)
    {
        List<Message> messages = GetMessages(contact1, contact2);

        foreach (Message message in messages)
        {
            connection.Delete(message);
        }
    }

    /// <summary>
    /// Saves a user's personal status message.
    /// </summary>
    /// <param name="userEmail"></param>
    /// <param name="personalMessage"></param>
    public void SavePersonalMessage(string userEmail, string personalMessage)
    {
        List<User> users = connection.Table<User>().Where(user => user.UserEmail == userEmail).ToList();

        foreach (User user in users)
        {
            user.PersonalMessage = personalMessage;
            SaveUser(user);
        }
    }

    /// <summary>
    /// Returns a contact's display picture.
    /// </summary>
    /// <param name="contactEmail"></param>
    /// <returns></returns>
    public DisplayPicture? GetContactDisplayPicture(string contactEmail)
    {
        return connection.Table<DisplayPicture>().LastOrDefault(picture => picture.ContactEmail == contactEmail && !picture.IsUserPicture);
    }

    /// <summary>
    /// Returns a user's display picture.
    /// </summary>
    /// <param name="contactEmail"></param>
    /// <returns></returns>
    public DisplayPicture? GetUserDisplayPicture(string contactEmail)
    {
        return connection.Table<DisplayPicture>().LastOrDefault(picture => picture.ContactEmail == contactEmail && picture.IsUserPicture);
    }

    /// <summary>
    /// Saves a user's display picture.
    /// </summary>
    /// <param name="picture"></param>
    /// <returns></returns>
    public int SaveDisplayPicture(DisplayPicture picture)
    {
        return connection.Insert(picture);
    }

    /// <summary>
    /// Deletes any display picture.
    /// </summary>
    /// <param name="picture"></param>
    /// <returns></returns>
    public int DeleteDisplayPicture(DisplayPicture picture)
    {
        return connection.Delete(picture);
    }

    /// <summary>
    /// Deletes every display picture associated with a contact.
    /// </summary>
    /// <param name="contactEmail"></param>
    public void DeleteContactDisplayPictures(string contactEmail)
    {
        List<DisplayPicture> pictures = connection.Table<DisplayPicture>().Where(picture => picture.ContactEmail == contactEmail && !picture.IsUserPicture).ToList();

        foreach (DisplayPicture picture in pictures)
        {
            DeleteDisplayPicture(picture);
        }
    }

    /// <summary>
    /// Deletes every display picture associated with a user account.
    /// </summary>
    /// <param name="userEmail"></param>
    public void DeleteUserDisplayPictures(string userEmail)
    {
        List<DisplayPicture> pictures = connection.Table<DisplayPicture>().Where(picture => picture.ContactEmail == userEmail && picture.IsUserPicture).ToList();

        foreach (DisplayPicture picture in pictures)
        {
            DeleteDisplayPicture(picture);
        }
    }
}
