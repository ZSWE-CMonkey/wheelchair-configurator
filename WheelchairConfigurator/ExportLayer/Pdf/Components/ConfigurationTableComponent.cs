using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WheelchairConfigurator.Export.ExportModel;

namespace WheelchairConfigurator.Export.Pdf.Components;

public class ConfigurationTableComponent : IComponent
{
    private readonly ConfigurationExportModel _model;

    public ConfigurationTableComponent(ConfigurationExportModel model)
    {
        _model = model;
    }

    public void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(50);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    // Add light padding to give it some breathing room
                    header.Cell().PaddingBottom(5).Text("Kategorie").SemiBold();
                    header.Cell().PaddingBottom(5).Text("Název komponenty").SemiBold();
                    header.Cell().PaddingBottom(5).AlignRight().Text("Kód položky").SemiBold();
                    header.Cell().PaddingBottom(5).AlignRight().Text("Ks").SemiBold();
                    header.Cell().PaddingBottom(5).AlignRight().Text("Cena").SemiBold();

                    // Bottom border under the header
                    header.Cell().ColumnSpan(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                });

                // Introduce an index to determine even/odd rows for zebra striping
                int index = 0;
                foreach (var item in _model.Items)
                {
                    // Alternating colors: white and very light grey
                    var backgroundColor = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    // Wrap cells to apply the background color
                    table.Cell().Background(backgroundColor).PaddingVertical(5).PaddingLeft(2).Text(item.CategoryName);
                    table.Cell().Background(backgroundColor).PaddingVertical(5).Text(item.ComponentName);

                    // TODO: Replace "888" with the real code here, e.g., item.ItemCode
                    table.Cell().Background(backgroundColor).PaddingVertical(5).AlignRight().Text(item.ItemCode);

                    table.Cell().Background(backgroundColor).PaddingVertical(5).AlignRight().Text(item.Quantity.ToString());
                    table.Cell().Background(backgroundColor).PaddingVertical(5).PaddingRight(2).AlignRight().Text($"${item.Price:N2}");

                    index++;
                }
            });

            // Total price at the bottom right
            column.Item().PaddingTop(15).AlignRight().Text(text =>
            {
                text.Span("Celková cena: ").FontSize(16).SemiBold();
                text.Span($"${_model.TotalPrice:N2}")
                    .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
            });
        });
    }
}