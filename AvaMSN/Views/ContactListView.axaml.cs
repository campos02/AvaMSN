using Avalonia.Controls;

namespace AvaMSN.Views;

public partial class ContactListView : UserControl
{
    public ContactListView()
    {
        InitializeComponent();
    }

    private void Button_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        presenceBox.IsDropDownOpen = false;
    }
}
