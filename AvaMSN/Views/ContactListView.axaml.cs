using Avalonia.Controls;
using AvaMSN.ViewModels;

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

    private async void Display_Picture_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);

        await (DataContext as ContactListViewModel).ChangeDisplayPicture(topLevel!);
    }
}
