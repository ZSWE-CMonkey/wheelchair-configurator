using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
//using Microsoft.Maui.Storage;
using System.Diagnostics;
using WheelchairConfigurator.Export.ExportModel;
using WheelchairConfigurator.Export.Pdf.Components;

namespace WheelchairConfigurator.Export.Pdf;

public class PdfBuilder : IExportFileBuilder
{
    public string Build(ConfigurationExportModel model)
    {
        string fileName = $"Configuration_{model.ConfigurationName.Replace(" ", "_")}.pdf";
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        // MAUI save location (cache directory)
        // string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(2, Unit.Centimetre);
                page.MarginTop(1.5f, Unit.Centimetre);
                page.MarginBottom(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));

                // 1 insert header at the top of the page
                page.Header().PaddingBottom(15).Component(new HeaderComponent());

                // 2 insert main content in the middle of the page
                page.Content().Column(column =>
                {
                    column.Item().Component(new MetadataComponent(model));
                    column.Item().Component(new ConfigurationTableComponent(model));
                    column.Item().PaddingTop(80).Component(new SignatureComponent("Podpis odpovědné osoby"));
                });

                // 3 insert footer at the bottom of the page
                page.Footer().PaddingTop(10).Component(new FooterComponent());
            });
        })

        .GeneratePdf(filePath);

        return filePath;
    }
}
