using Avalonia.Controls;
using Avalonia.Input;
using System.Reactive;

namespace AvaMSN.Views;

public partial class ConversationWindow : Window
{
    public ConversationWindow()
    {
        InitializeComponent();
    }

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sendButton.Command == null || e.Key != Key.Enter)
            return;

        sendButton.Command.Execute(new Unit { });
    }

    private void ItemsControl_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        scrollViewer?.ScrollToEnd();
    }
}
