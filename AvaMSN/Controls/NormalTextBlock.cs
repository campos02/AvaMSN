using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaMSN.Controls;

/// <summary>
/// TextBox control that removes any decorations, colors, different weights or styles added.
/// </summary>
public class NormalTextBlock : TextBlock
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        FontWeight = FontWeight.Normal;
        FontStyle = FontStyle.Normal;
        TextDecorations = null;
        Foreground = Brushes.Black;
    }
}