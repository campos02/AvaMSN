using AvaMSN.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AvaMSN;

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

    public List<User> GetUsers()
    {
        return connection.Table<User>().ToList();
    }

    public int SaveUser(User user)
    {
        if (user.ID != 0)
            return connection.Update(user);

        else
            return connection.Insert(user);
    }

    public int DeleteUser(User user)
    {
        return connection.Delete(user);
    }

    public List<Message> GetMessages()
    {
        return connection.Table<Message>().ToList();
    }

    public List<Message> GetMessages(string contact1, string contact2)
    {
        return connection.Table<Message>().Where(message => (message.Sender == contact1 || message.Recipient == contact1)
                                                 && (message.Recipient == contact2 || message.Sender == contact2)).ToList();
    }

    public int SaveMessage(Message message)
    {
        return connection.Insert(message);
    }

    public int DeleteMessage(Message message)
    {
        return connection.Delete(message);
    }

    public void DeleteMessages(string contact1, string contact2)
    {
        List<Message> messages = GetMessages(contact1, contact2);

        foreach (Message message in messages)
        {
            connection.Delete(message);
        }
    }

    public void SavePersonalMessage(string userEmail, string personalMessage)
    {
        List<User> users = connection.Table<User>().Where(user => user.UserEmail == userEmail).ToList();

        foreach (User user in users)
        {
            user.PersonalMessage = personalMessage;
            SaveUser(user);
        }
    }

    public DisplayPicture? GetContactDisplayPicture(string contactEmail)
    {
        return connection.Table<DisplayPicture>().LastOrDefault(picture => picture.ContactEmail == contactEmail && !picture.IsUserPicture);
    }

    public DisplayPicture? GetUserDisplayPicture(string contactEmail)
    {
        return connection.Table<DisplayPicture>().LastOrDefault(picture => picture.ContactEmail == contactEmail && picture.IsUserPicture);
    }

    public int SaveDisplayPicture(DisplayPicture picture)
    {
        return connection.Insert(picture);
    }

    public int DeleteDisplayPicture(DisplayPicture picture)
    {
        return connection.Delete(picture);
    }

    public void DeleteContactDisplayPictures(string contactEmail)
    {
        List<DisplayPicture> pictures = connection.Table<DisplayPicture>().Where(picture => picture.ContactEmail == contactEmail && !picture.IsUserPicture).ToList();

        foreach (DisplayPicture picture in pictures)
        {
            DeleteDisplayPicture(picture);
        }
    }

    public void DeleteUserDisplayPictures(string userEmail)
    {
        List<DisplayPicture> pictures = connection.Table<DisplayPicture>().Where(picture => picture.ContactEmail == userEmail && picture.IsUserPicture).ToList();

        foreach (DisplayPicture picture in pictures)
        {
            DeleteDisplayPicture(picture);
        }
    }
}
