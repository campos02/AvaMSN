using AvaMSN.Models;
using AvaMSN.MSNP.PresenceStatus;
using System.Collections.ObjectModel;

namespace AvaMSN.ViewModels.Design;

public class DesignContactListViewModel : ContactListViewModel
{
    public DesignContactListViewModel()
    {
        ListData = new ContactListData()
        {
            Profile = new Profile()
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
