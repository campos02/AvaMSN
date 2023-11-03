using System.Collections.ObjectModel;

namespace AvaMSN.Models;

public class ContactGroup
{
    public ObservableCollection<Contact> Contacts { get; set; } = new ObservableCollection<Contact>();

    public string Name { get; set; } = string.Empty;

    public ContactGroup(string name, ObservableCollection<Contact> contacts) 
    {
        Contacts = contacts;
        Name = name;
    }
}
