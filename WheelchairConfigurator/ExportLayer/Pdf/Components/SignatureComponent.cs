using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using WheelchairConfigurator.Export.Pdf.Interfaces;

namespace WheelchairConfigurator.Export.Pdf.Components;

/// <summary>
/// Component responsible for rendering a signature section in the PDF document, typically used for signing off on the configuration by a specialist or client.
/// </summary>
public class SignatureComponent : IPdfComponent
{
    private readonly string _title;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignatureComponent"/> class with an optional title for the signature section.
    /// </summary>
    /// <param name="title">The text displayed directly below the signature line (default is "Podpis").</param>
    public SignatureComponent(string title = "Podpis")
    {
        _title = title;
    }

    /// <summary>
    /// Renders the signature block into the provided document section.
    /// Automatically adds vertical spacing above the block to separate it from the preceding content.
    /// </summary>
    /// <param name="section">The MigraDoc section where the content will be added.</param>
    public void Render(Section section)
    {
        // 1. Add visual spacing before the signature block
        var spacer = section.AddParagraph();
        spacer.Format.SpaceBefore = Unit.FromPoint(80);

        // 2. Setup a layout table (Left: Place/Date | Right: Signature)
        var table = section.AddTable();
        table.Borders.Visible = false;

        table.AddColumn(Unit.FromCentimeter(10));
        table.AddColumn(Unit.FromCentimeter(7));

        var row1 = table.AddRow();
        row1.VerticalAlignment = VerticalAlignment.Bottom;
        row1.Format.Font.Size = 10;

        // 3. Left Column: Location and Date placeholders
        var p1 = row1.Cells[0].AddParagraph();
        p1.AddText("V ");
        p1.AddText(".............................");
        p1.AddText("   dne ");
        p1.AddText(".............................");

        // 4. Right Column: Signature line
        var p2 = row1.Cells[1].AddParagraph();
        p2.Format.Alignment = ParagraphAlignment.Center;
        p2.AddText("............................................");

        // 5. Add the signature title/role underneath the line
        var row2 = table.AddRow();
        row2.TopPadding = Unit.FromPoint(2);

        var labelCell = row2.Cells[1];
        labelCell.Format.Alignment = ParagraphAlignment.Center;

        var labelText = labelCell.AddParagraph();
        var formatted = labelText.AddFormattedText(_title);
        formatted.Font.Size = 9;
        formatted.Font.Color = PdfDocumentColors.GreyDark;
    }
}
