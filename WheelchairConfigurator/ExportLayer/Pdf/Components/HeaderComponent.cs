using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace WheelchairConfigurator.Export.Pdf.Components;

/// <summary>
/// PDF component responsible for rendering the document header.
/// Features a two-column layout containing the document titles and an optional company logo.
/// </summary>
public class HeaderComponent
{
    /// <summary>The raw byte array of the logo image.</summary>
    private readonly byte[]? _logoBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderComponent"/> class.
    /// </summary>
    /// <param name="logoBytes">Optional byte array containing the logo image data.</param>
    public HeaderComponent(byte[]? logoBytes)
    {
        _logoBytes = logoBytes;
    }

    /// <summary>
    /// Renders the header layout consisting of a title, subtitle, and an embedded base64 logo.
    /// </summary>
    /// <param name="header">The MigraDoc header section where the content will be added.</param>
    public void Render(HeaderFooter header)
    {
        var table = header.AddTable();
        table.Borders.Visible = false;

        table.AddColumn(Unit.FromCentimeter(12));
        table.AddColumn(Unit.FromCentimeter(5));

        var row = table.AddRow();
        row.VerticalAlignment = VerticalAlignment.Top;

        // ---------------------------------------------------------------------
        // 1. Text Section (Left)
        // ---------------------------------------------------------------------
        var textCell = row.Cells[0];
        var title = textCell.AddParagraph("Wheelchair Configuration");
        title.Format.Font.Size = 18;
        title.Format.Font.Bold = true;
        title.Format.Font.Color = new Color(30, 144, 255);

        var subtitle = textCell.AddParagraph("Vygenerováno systémem");
        subtitle.Format.Font.Size = 10;
        subtitle.Format.Font.Color = PdfDocumentColors.GreyDark;

        // ---------------------------------------------------------------------
        // 2. Logo Section (Right)
        // ---------------------------------------------------------------------
        var logoCell = row.Cells[1];
        logoCell.Format.Alignment = ParagraphAlignment.Right;
        logoCell.Format.RightIndent = Unit.FromCentimeter(1);
        logoCell.Format.SpaceBefore = Unit.FromCentimeter(0.4);

        if (_logoBytes != null && _logoBytes.Length > 0)
        {
            string base64String = Convert.ToBase64String(_logoBytes);
            string imageSource = "base64:" + base64String;

            var image = logoCell.AddParagraph().AddImage(imageSource);
            image.Width = Unit.FromCentimeter(2.5);
            image.LockAspectRatio = true;
        }
    }
}