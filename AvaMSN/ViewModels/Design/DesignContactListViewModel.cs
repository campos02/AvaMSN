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
                Presence = PresenceStatus.GetFullName(PresenceStatus.Available)
            },

            ContactGroups = new ObservableCollection<ContactGroup>()
            {
                new ContactGroup("Available", new ObservableCollection<Contact>()
                {
                    new Contact()
                    {
                        DisplayName = "Available Contact",
                        Presence = PresenceStatus.GetFullName(PresenceStatus.Available)
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
