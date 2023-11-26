using System.Collections.ObjectModel;

namespace AvaMSN.Models;

public class ContactGroup(string name, ObservableCollection<Contact> contacts)
{
    public ObservableCollection<Contact> Contacts { get; set; } = contacts;
    public string Name { get; set; } = name;
}
