using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Shoootz.Views.Extensions;

internal static class TextHighlightExtension
{
    public static TextBlock Highlight(this string template, string highlightText)
    {
        string[] parts = template.Split("{0}", 2);

        TextBlock textBlock = new() { FontSize = 16, TextWrapping = TextWrapping.Wrap };

        if (parts.Length == 2)
        {
            textBlock.Inlines =
            [
                new Run(parts[0]),
                new Bold
                {
                    FontWeight = FontWeight.Bold,
                    Inlines = [new Run(highlightText) { FontSize = 16, FontWeight = FontWeight.Bold }],
                },
                new Run(parts[1]),
            ];
        }
        else
        {
            textBlock.Inlines = [new Run(template)];
        }

        return textBlock;
    }
}
