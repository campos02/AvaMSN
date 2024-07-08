using System.Collections.ObjectModel;

namespace AvaMSN.Models;

/// <summary>
/// Represents contact groups.
/// </summary>
/// <param name="name">Group name.</param>
/// <param name="contacts">List of contacts for group.</param>
public class ContactGroup(string name, ObservableCollection<Contact?> contacts)
{
    public ObservableCollection<Contact?> Contacts { get; set; } = contacts;
    public string Name { get; set; } = name;
}
