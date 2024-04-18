using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaMSN.Controls;

public class NormalTextBlock : TextBlock
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        FontWeight = FontWeight.Normal;
        FontStyle = FontStyle.Normal;
        TextDecorations = null;
    }
}