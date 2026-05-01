using System.Linq;
using MigraDoc.DocumentObjectModel;

namespace WheelchairConfigurator.Export.Tests;

public static class MigraDocExtensions
{
    public static string GetRawText(this Paragraph paragraph)
    {
        var text = "";

        foreach (var element in paragraph.Elements)
        {
            if (element is Text t)
            {
                text += t.Content;
            }
            else if (element is FormattedText ft)
            {
                text += string.Concat(ft.Elements.OfType<Text>().Select(inner => inner.Content));
            }
        }

        return text;
    }
}