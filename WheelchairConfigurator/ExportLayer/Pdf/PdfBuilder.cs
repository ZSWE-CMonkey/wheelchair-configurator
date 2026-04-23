using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using WheelchairConfigurator.Export.ExportModel;
using WheelchairConfigurator.Export.Pdf.Components;

namespace WheelchairConfigurator.Export.Pdf;

/// <summary>
/// Orchestrates the creation of the PDF document by assembling individual components
/// and rendering the final output into a byte array.
/// </summary>
public class PdfBuilder : IExportFileBuilder
{
    private readonly byte[]? _logoBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfBuilder"/> class with an optional logo image.
    /// The logo will be displayed in the header of the PDF document if provided.
    /// </summary>
    /// <param name="logoBytes">The byte array containing the logo image data.</param>
    public PdfBuilder(byte[]? logoBytes = null)
    {
        _logoBytes = logoBytes;
    }

    /// <summary>
    /// Builds the configuration export PDF document.
    /// </summary>
    /// <param name="model">The data model containing configuration details to be exported.</param>
    /// <returns>A byte array representing the generated PDF file.</returns>
    public byte[] Build(ConfigurationExportModel model)
    {
        var document = CreateDocument();
        var section = document.AddSection();
        ConfigureSection(section);

        // 1. Page Header
        new HeaderComponent(_logoBytes).Render(section.Headers.Primary);

        // 2. Page Content
        new MetadataComponent(model).Render(section);
        new ConfigurationTableComponent(model).Render(section);
        new SignatureComponent("Podpis odpovědné osoby").Render(section);

        // 3. Page Footer
        new FooterComponent().Render(section.Footers.Primary);

        return ConvertDocumentToBytes(document); ;
    }

    /// <summary>
    /// Initializes the base MigraDoc document structure and defines the default styling.
    /// </summary>
    /// <returns>A new <see cref="Document"/> instance.</returns>
    private static Document CreateDocument()
    {
        var document = new Document();
        document.Info.Title = "Wheelchair Configuration";
        var normalStyle = document.Styles["Normal"]!;
        normalStyle.Font.Name = "Roboto";
        normalStyle.Font.Size = 12;

        return document;
    }

    /// <summary>
    /// Configures the page dimensions, layout format, and margins for the document section.
    /// </summary>
    /// <param name="section">The section to configure.</param>
    private static void ConfigureSection(Section section)
    {
        var setup = section.PageSetup;
        setup.PageFormat = PageFormat.A4;
        setup.LeftMargin = Unit.FromCentimeter(2);
        setup.RightMargin = Unit.FromCentimeter(2);
        setup.TopMargin = Unit.FromCentimeter(3);
        setup.BottomMargin = Unit.FromCentimeter(1);
        setup.HeaderDistance = Unit.FromCentimeter(0.5);
        setup.FooterDistance = Unit.FromCentimeter(0.5);
    }

    /// <summary>
    /// Renders the MigraDoc document and saves it to an in-memory byte array.
    /// </summary>
    /// <param name="document">The fully assembled MigraDoc document.</param>
    /// <returns>The raw bytes of the generated PDF file.</returns>
    private static byte[] ConvertDocumentToBytes(Document document)
    {
        var renderer = new PdfDocumentRenderer
        {
            Document = document
        };
        renderer.RenderDocument();

        using (var stream = new MemoryStream())
        {
            renderer.PdfDocument.Save(stream, false);
            return stream.ToArray();
        }
    }
}