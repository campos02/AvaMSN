using Avalonia.Controls;

namespace AvaMSN.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void Button_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        userBox.IsDropDownOpen = false;
    }

    private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        userBox.SelectedIndex = -1;
    }
}
