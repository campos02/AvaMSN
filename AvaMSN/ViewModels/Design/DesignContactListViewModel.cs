using AvaMSN.Models;
using System.Collections.ObjectModel;
using AvaMSN.MSNP.Utils;
using Contact = AvaMSN.Models.Contact;

namespace AvaMSN.ViewModels.Design;

public class DesignContactListViewModel : ContactListViewModel
{
    public DesignContactListViewModel()
    {
        ListData = new ContactListData()
        {
            Profile = new Models.Profile()
            {
                DisplayName = "Testing",
                Presence = PresenceStatus.GetFullName(PresenceStatus.Online)
            },

            ContactGroups = new ObservableCollection<ContactGroup>()
            {
                new ContactGroup("Online", new ObservableCollection<Contact>()
                {
                    new Contact()
                    {
                        DisplayName = "Online Contact",
                        Presence = PresenceStatus.GetFullName(PresenceStatus.Online),
                        NewMessages = true
                    }
                }),

                new ContactGroup("Offline", new ObservableCollection<Contact>()
                {
                    new Contact()
                    {
                        DisplayName = "Offline Contact"
                    }
                }),
            }
        };
    }
}
