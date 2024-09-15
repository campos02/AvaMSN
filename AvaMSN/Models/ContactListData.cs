using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaMSN.MSNP.Models;
using AvaMSN.MSNP.NotificationServer.Contacts;
using AvaMSN.MSNP.NotificationServer.UserProfile;

namespace AvaMSN.Models;

/// <summary>
/// Contains contact groups, starting code and NS event handlers.
/// </summary>
public class ContactListData
{
    public ContactActions? ContactActions { get; set; }
    public UserProfile? UserProfile { get; set; }
    public User User { get; set; } = new User();
    public ObservableCollection<ContactGroup>? ContactGroups { get; set; }
    public List<Conversation> Conversations { get; set; } = new List<Conversation>();

    public static Presence[] Statuses { get; } =
    {
        new Presence
        {
            Status = PresenceStatus.GetFullName(PresenceStatus.Online),
            Color = "LimeGreen",
            ShortName = PresenceStatus.Online
        },

        new Presence
        {
            Status = PresenceStatus.GetFullName(PresenceStatus.Busy),
            Color = "IndianRed",
            ShortName = PresenceStatus.Busy
        },

        new Presence
        {
            Status = PresenceStatus.GetFullName(PresenceStatus.Away),
            Color = "DarkOrange",
            ShortName = PresenceStatus.Away
        },

        new Presence
        {
            Status = PresenceStatus.GetFullName(PresenceStatus.Invisible),
            Color = "Gray",
            ShortName = PresenceStatus.Invisible
        }
    };

    /// <summary>
    /// Gets contact data from Notification Server and creates contact groups with it.
    /// </summary>
    public void GetData()
    {
        if (ContactActions == null)
            return;

        // Get user profile data
        User.DisplayName = ContactActions.User!.DisplayName;
        User.Email = ContactActions.User.Email;
        User.PersonalMessage = ContactActions.User.PersonalMessage;
        User.Presence = PresenceStatus.GetFullName(ContactActions.User.Presence);

        ContactGroups =
        [
            new ContactGroup("Online", []),
            new ContactGroup("Offline", [])
        ];

        foreach (MSNP.Models.Contact contact in ContactActions.ContactList)
        {
            // Only add contacts in forward list
            if (!contact.InLists.Forward)
                continue;

            if (contact.Presence != PresenceStatus.Offline)
            {
                foreach (ContactGroup group in ContactGroups)
                {
                    if (group.Name != "Offline")
                    {
                        group.Contacts.Add(new Contact
                        {
                            DisplayName = contact.DisplayName,
                            Email = contact.Email,
                            PersonalMessage = contact.PersonalMessage,
                            Presence = PresenceStatus.GetFullName(contact.Presence),
                            PresenceColor = GetStatusColor(contact.Presence),
                            Blocked = contact.InLists.Block
                        });
                    }
                }
            }

            else
            {
                foreach (ContactGroup group in ContactGroups)
                {
                    if (group.Name != "Online")
                    {
                        group.Contacts.Add(new Contact
                        {
                            DisplayName = contact.DisplayName,
                            Email = contact.Email,
                            PersonalMessage = contact.PersonalMessage,
                            Presence = PresenceStatus.GetFullName(contact.Presence),
                            PresenceColor = GetStatusColor(contact.Presence),
                            Blocked = contact.InLists.Block
                        });
                    }
                }
            }
        }

        ContactActions.Incoming!.PresenceChanged += ContactActions_PresenceChanged;
        ContactActions.Incoming.PersonalMessageChanged += ContactActions_PersonalMessageChanged;
    }

    public static string GetStatusColor(string status) => status switch
    {
        PresenceStatus.Online => "LimeGreen",
        PresenceStatus.Busy => "IndianRed",
        PresenceStatus.Away => "DarkOrange",
        PresenceStatus.Idle => "DarkOrange",
        PresenceStatus.BeRightBack => "DarkOrange",
        PresenceStatus.OnThePhone => "IndianRed",
        PresenceStatus.OutToLunch => "DarkOrange",
        _ => "Gray"
    };

    /// <summary>
    /// Handles presence events, changing groups according to a contact's new status.
    /// </summary>
    private void ContactActions_PresenceChanged(object? sender, PresenceEventArgs e)
    {
        if (ContactGroups == null)
            return;

        Contact? contact = null;

        // Select contact
        foreach (ContactGroup group in ContactGroups)
        {
            contact = group.Contacts.FirstOrDefault(c => c?.Email == e.Email);
            if (contact != null)
                break;
        }

        if (contact == null)
            return;

        if (e.Presence != PresenceStatus.Offline)
        {
            // If the previous status was offline...
            if (contact.Presence == PresenceStatus.GetFullName(PresenceStatus.Offline))
            {
                // Remove from the offline group and add to the online group
                foreach (ContactGroup group in ContactGroups)
                {
                    if (group.Name == "Offline")
                        group.Contacts.Remove(contact);

                    else if (group.Name == "Online")
                        group.Contacts.Add(contact);
                }
            }
        }

        else
        {
            // Remove from online group and add to offline group
            foreach (ContactGroup group in ContactGroups)
            {
                if (group.Name == "Offline")
                    group.Contacts.Add(contact);

                else if (group.Name == "Online")
                    group.Contacts.Remove(contact);
            }
        }

        // Set new presence and display picture
        foreach (ContactGroup group in ContactGroups)
        {
            contact = group.Contacts.FirstOrDefault(c => c?.Email == e.Email);
            if (contact != null)
            {
                contact.Presence = PresenceStatus.GetFullName(e.Presence);
                contact.PresenceColor = GetStatusColor(e.Presence);

                if (e.HasDisplayPicture)
                {
                    if (e.DisplayPictureHash != null)
                        contact.DisplayPictureHash = e.DisplayPictureHash;
                }
                else
                {
                    // Remove display picture if the contact doesn't have it anymore
                    contact.DisplayPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AvaMSN.Shared/Assets/default-display-picture.png")));
                }
            }
        }
    }

    private void ContactActions_PersonalMessageChanged(object? sender, PersonalMessageEventArgs e)
    {
        if (ContactGroups == null)
            return;

        foreach (ContactGroup group in ContactGroups)
        {
            Contact? contact = group.Contacts.FirstOrDefault(c => c?.Email == e.Email);
            if (contact != null)
                contact.PersonalMessage = e.PersonalMessage;
        }
    }
}
