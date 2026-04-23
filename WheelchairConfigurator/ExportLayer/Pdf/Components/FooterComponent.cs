using MigraDoc.DocumentObjectModel;
using WheelchairConfigurator.Export.Pdf.Interfaces;

namespace WheelchairConfigurator.Export.Pdf.Components;

/// <summary>
/// Represents the footer component of the PDF document, responsible for rendering page numbers and generation timestamp.
/// </summary>
public class FooterComponent : IPdfFooterComponent
{
    /// <summary>
    /// Renders the footer content into the provided MigraDoc footer section.
    /// </summary>
    /// <param name="footer">The MigraDoc footer section where the content will be added.</param>
    public void Render(HeaderFooter footer)
    {
        var paragraph = footer.AddParagraph();
        paragraph.Format.Alignment = ParagraphAlignment.Center;
        paragraph.Format.Font.Size = 9;
        paragraph.Format.Font.Color = PdfDocumentColors.GreyMedium;
        paragraph.Format.SpaceBefore = Unit.FromPoint(10);

        paragraph.AddText($"Generated on {DateTime.Now:g} | Page ");
        paragraph.AddPageField();
        paragraph.AddText(" of ");
        paragraph.AddNumPagesField();
    }
}
