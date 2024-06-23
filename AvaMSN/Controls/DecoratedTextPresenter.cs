using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using AvaMSN.Utils;

namespace AvaMSN.Controls;

public class DecoratedTextPresenter : TextPresenter
{
    private Size _constraint;
    
    /// <summary>
    /// Defines the <see cref="TextDecorations"/> property.
    /// </summary>
    public static readonly StyledProperty<TextDecorationCollection?> TextDecorationsProperty =
        Inline.TextDecorationsProperty.AddOwner<DecoratedTextBox>();
    
    /// <summary>
    /// Gets or sets the text decorations.
    /// </summary>
    public TextDecorationCollection? TextDecorations
    {
        get => GetValue(TextDecorationsProperty);
        set => SetValue(TextDecorationsProperty, value);
    }
    
    protected override Size MeasureOverride(Size availableSize)
    {
        _constraint = availableSize;
        return base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _constraint = new Size(Math.Ceiling(finalSize.Width), double.PositiveInfinity);
        return base.ArrangeOverride(finalSize);
    }

    /// <summary>
    /// Creates the <see cref="TextLayout"/> used to render the text.
    /// Modified from the original to work with text decorations.
    /// </summary>
    /// <param name="constraint">The constraint of the text.</param>
    /// <param name="text">The text to format.</param>
    /// <param name="typeface"></param>
    /// <param name="textStyleOverrides"></param>
    /// <param name="textDecorations"></param>
    /// <returns>A <see cref="TextLayout"/> object.</returns>
    private TextLayout CreateTextLayoutInternal(Size constraint, string? text, Typeface typeface,
        IReadOnlyList<ValueSpan<TextRunProperties>>? textStyleOverrides,
        TextDecorationCollection? textDecorations)
    {
        var foreground = Foreground;
        var maxWidth = MathUtilities.IsZero(constraint.Width) ? double.PositiveInfinity : constraint.Width;
        var maxHeight = MathUtilities.IsZero(constraint.Height) ? double.PositiveInfinity : constraint.Height;

        var textLayout = new TextLayout(text, typeface, FontSize, foreground, TextAlignment,
            TextWrapping, maxWidth: maxWidth, maxHeight: maxHeight, textStyleOverrides: textStyleOverrides,
            flowDirection: FlowDirection, lineHeight: LineHeight, letterSpacing: LetterSpacing,
            textDecorations: textDecorations);

        return textLayout;
    }
    
    private static string? GetCombinedText(string? text, int caretIndex, string? preEditText)
    {
        if (string.IsNullOrEmpty(preEditText))
        {
            return text;
        }

        if (string.IsNullOrEmpty(text))
        {
            return preEditText;
        }

        var sb = StringBuilderCache.Acquire(text.Length + preEditText.Length);

        sb.Append(text.Substring(0, caretIndex));
        sb.Insert(caretIndex, preEditText);
        sb.Append(text.Substring(caretIndex));

        return StringBuilderCache.GetStringAndRelease(sb);
    }

    protected override TextLayout CreateTextLayout()
    {
        TextLayout result;

        var caretIndex = CaretIndex;
        var preEditText = PreeditText;
        var text = GetCombinedText(Text, caretIndex, preEditText);
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var textDecorations = TextDecorations;
        var selectionStart = SelectionStart;
        var selectionEnd = SelectionEnd;
        var start = Math.Min(selectionStart, selectionEnd);
        var length = Math.Max(selectionStart, selectionEnd) - start;

        IReadOnlyList<ValueSpan<TextRunProperties>>? textStyleOverrides = null;

        var foreground = Foreground;

        if (!string.IsNullOrEmpty(preEditText))
        {
            var preEditHighlight = new ValueSpan<TextRunProperties>(caretIndex, preEditText.Length,
                new GenericTextRunProperties(typeface, FontSize,
                    foregroundBrush: foreground,
                    textDecorations: Avalonia.Media.TextDecorations.Underline));

            textStyleOverrides = new[]
            {
                preEditHighlight
            };
        }
        else
        {
            if (length > 0 && SelectionForegroundBrush != null)
            {
                textStyleOverrides = new[]
                {
                    new ValueSpan<TextRunProperties>(start, length,
                    new GenericTextRunProperties(typeface, FontSize,
                        foregroundBrush: SelectionForegroundBrush))
                };
            }
        }

        if (PasswordChar != default(char) && !RevealPassword)
        {
            result = CreateTextLayoutInternal(_constraint, new string(PasswordChar, text?.Length ?? 0), typeface,
                textStyleOverrides, textDecorations: textDecorations);
        }
        else
        {
            result = CreateTextLayoutInternal(_constraint, text, typeface, textStyleOverrides, textDecorations);
        }

        return result;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property.Name == nameof(TextDecorations))
            InvalidateTextLayout();
    }
}