﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaMSN.MSNP;
using AvaMSN.MSNP.PresenceStatus;

namespace AvaMSN.Models;

public class ContactListData
{
    public NotificationServer? NotificationServer { get; set; }
    public Profile Profile { get; set; } = new();
    public ObservableCollection<ContactGroup>? ContactGroups { get; set; }

    public enum DefaultGroupIndex
    {
        Available = 0,
        Offline = 1
    }

    public static Presence[] Statuses { get; } =
    {
        new Presence()
        {
            Status = PresenceStatus.GetFullName(PresenceStatus.Available),
            Color = "LimeGreen",
            ShortName = PresenceStatus.Available
        },

        new Presence()
        {
            Status = PresenceStatus.GetFullName(PresenceStatus.Busy),
            Color = "IndianRed",
            ShortName = PresenceStatus.Busy
        },

        new Presence()
        {
            Status = PresenceStatus.GetFullName(PresenceStatus.Away),
            Color = "DarkOrange",
            ShortName = PresenceStatus.Away
        },

        new Presence()
        {
            Status = PresenceStatus.GetFullName(PresenceStatus.Invisible),
            Color = "Gray",
            ShortName = PresenceStatus.Invisible
        }
    };

    public void GetProperties()
    {
        if (NotificationServer == null)
            return;

        Profile.DisplayName = NotificationServer.Profile.DisplayName;
        Profile.Email = NotificationServer.Profile.Email;
        Profile.PersonalMessage = NotificationServer.Profile.PersonalMessage;
        Profile.Presence = PresenceStatus.GetFullName(NotificationServer.Profile.Presence);

        ContactGroups =
        [
            new ContactGroup("Available", []),
            new ContactGroup("Offline", [])
        ];

        foreach (MSNP.Contact contact in NotificationServer.ContactList.Contacts)
        {
            if (!contact.InLists.Forward)
                continue;

            if (contact.Presence != PresenceStatus.Offline)
            {
                ContactGroups[(int)DefaultGroupIndex.Available].Contacts.Add(new Contact()
                {
                    DisplayName = contact.DisplayName,
                    Email = contact.Email,
                    PersonalMessage = contact.PersonalMessage,
                    Presence = PresenceStatus.GetFullName(contact.Presence),
                    PresenceColor = GetStatusColor(contact.Presence)
                });
            }

            else
            {
                ContactGroups[(int)DefaultGroupIndex.Offline].Contacts.Add(new Contact()
                {
                    DisplayName = contact.DisplayName,
                    Email = contact.Email,
                    PersonalMessage = contact.PersonalMessage,
                    Presence = PresenceStatus.GetFullName(contact.Presence),
                    PresenceColor = GetStatusColor(contact.Presence)
                });
            }
        }

        NotificationServer.PresenceChanged += NotificationServer_PresenceChanged;
        NotificationServer.PersonalMessageChanged += NotificationServer_PersonalMessageChanged;
    }

    public string GetStatusColor(string status) => status switch
    {
        PresenceStatus.Available => "LimeGreen",
        PresenceStatus.Busy => "IndianRed",
        PresenceStatus.Away => "DarkOrange",
        PresenceStatus.Idle => "DarkOrange",
        PresenceStatus.BeRightBack => "DarkOrange",
        PresenceStatus.OnThePhone => "IndianRed",
        PresenceStatus.OutToLunch => "DarkOrange",
        _ => "Gray"
    };

    private void NotificationServer_PresenceChanged(object? sender, PresenceEventArgs e)
    {
        if (NotificationServer == null || ContactGroups == null)
            return;

        Contact? contact;

        if (e.Presence != PresenceStatus.Offline)
        {
            contact = ContactGroups[(int)DefaultGroupIndex.Available].Contacts.FirstOrDefault(c => c.Email == e.Email);

            if (contact == null)
            {
                contact = ContactGroups[(int)DefaultGroupIndex.Offline].Contacts.FirstOrDefault(c => c.Email == e.Email);

                if (contact == null)
                    return;

                ContactGroups[(int)DefaultGroupIndex.Offline].Contacts.Remove(contact);
                ContactGroups[(int)DefaultGroupIndex.Available].Contacts.Add(contact);
            }
        }

        else
        {
            contact = ContactGroups[(int)DefaultGroupIndex.Offline].Contacts.FirstOrDefault(c => c.Email == e.Email);

            if (contact == null)
            {
                contact = ContactGroups[(int)DefaultGroupIndex.Available].Contacts.FirstOrDefault(c => c.Email == e.Email);

                if (contact == null)
                    return;

                ContactGroups[(int)DefaultGroupIndex.Available].Contacts.Remove(contact);
                ContactGroups[(int)DefaultGroupIndex.Offline].Contacts.Add(contact);
            }
        }

        foreach (ContactGroup group in ContactGroups)
        {
            contact = group.Contacts.FirstOrDefault(c => c.Email == e.Email);

            if (contact != null)
            {
                contact.Presence = PresenceStatus.GetFullName(e.Presence);
                contact.PresenceColor = GetStatusColor(e.Presence);

                if (!e.HasDisplayPicture)
                    contact.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN/Assets/default-display-picture.png")));
            }
        }
    }

    private void NotificationServer_PersonalMessageChanged(object? sender, PersonalMessageEventArgs e)
    {
        if (NotificationServer == null || ContactGroups == null)
            return;

        Contact? contact;

        foreach (ContactGroup group in ContactGroups)
        {
            contact = group.Contacts.FirstOrDefault(c => c.Email == e.Email);

            if (contact != null)
                contact.PersonalMessage = e.PersonalMessage;
        }
    }
}
