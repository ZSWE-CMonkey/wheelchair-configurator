using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace WheelchairConfigurator.Export.Pdf.Components
{
    public class SignatureComponent : IComponent
    {
        private readonly string _title;

        public SignatureComponent(string title = "Podpis")
        {
            _title = title;
        }

        public void Compose(IContainer container)
        {
            container.PaddingTop(40).Column(col =>
            {
                col.Item().Text(t =>
                {
                    t.Span("V ").FontSize(10);
                    t.Span("............................").FontSize(10);
                    t.Span("  dne ").FontSize(10);
                    t.Span("............................").FontSize(10);

                    t.Span("                                           ").FontSize(10);
                    t.Span("................................................").FontSize(10);
                });

                col.Item().Row(row =>
                {
                    row.RelativeItem();
                    row.ConstantItem(120).PaddingTop(2).AlignCenter()
                       .Text(_title).FontSize(9).Italic().FontColor(Colors.Grey.Darken2);
                    row.ConstantItem(30);
                });

            });
        }
    }
}