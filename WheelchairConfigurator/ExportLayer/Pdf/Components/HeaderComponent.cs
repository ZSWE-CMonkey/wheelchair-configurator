using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace WheelchairConfigurator.Export.Pdf.Components;

public class HeaderComponent : IComponent
{
    public void Compose(IContainer container)
    {
        container.Row(row =>
        {
            //Left side: Title and subtitle
            row.RelativeItem().Column(column =>
            {
                column.Item()
                    .Text("Wheelchair Configuration")
                    .FontSize(24)
                    .SemiBold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item()
                    .Text("Premium Mobility Solutions")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Medium);
            });

            // Right side: Placeholder for logo or image
            row.ConstantItem(50)
   .Image(@"C:\Projects\vozikyDBTest\WheelchairConfigurator.Export\Assets\vibrant-cheerful-blue-dolphin-leaping-600nw-2732524075.webp")
   .FitArea();
        });
    }
}