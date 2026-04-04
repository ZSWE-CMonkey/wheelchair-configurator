using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace WheelchairConfigurator.Export.Pdf.Components;

public class FooterComponent : IComponent
{
    public void Compose(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span($"Generated on {System.DateTime.Now:g} | ").FontSize(9).FontColor(Colors.Grey.Medium);
            text.Span("Page ").FontSize(9).FontColor(Colors.Grey.Medium);
            text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
            text.Span(" of ").FontSize(9).FontColor(Colors.Grey.Medium);
            text.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
        });
    }
}