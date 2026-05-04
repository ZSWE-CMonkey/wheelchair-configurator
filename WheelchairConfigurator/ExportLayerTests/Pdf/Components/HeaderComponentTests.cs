using System;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using WheelchairConfigurator.Export.Pdf.Components;
using WheelchairConfigurator.Export.Pdf;
using WheelchairConfigurator.Export.Tests;
using Xunit;

namespace WheelchairConfigurator.Export.Tests.Pdf.Components;

public class HeaderComponentTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static HeaderFooter CreateHeader()
    {
        var document = new Document();
        var section = document.AddSection();
        return section.Headers.Primary;
    }

    /// <summary>
    /// Minimal 1x1 transparent PNG — smallest valid image for logo tests.
    /// </summary>
    private static byte[] CreateMinimalPngBytes()
    {
        return Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");
    }

    // -------------------------------------------------------------------------
    // Layout table structure
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_AddsTableWithTwoColumns()
    {
        var header = CreateHeader();
        new HeaderComponent(null).Render(header);

        var table = header.Elements.OfType<Table>().Single();
        Assert.Equal(2, table.Columns.Count);
    }

    [Fact]
    public void Render_TableBordersAreNotVisible()
    {
        var header = CreateHeader();
        new HeaderComponent(null).Render(header);

        var table = header.Elements.OfType<Table>().Single();
        Assert.False(table.Borders.Visible);
    }

    [Fact]
    public void Render_TableHasExactlyOneRow()
    {
        var header = CreateHeader();
        new HeaderComponent(null).Render(header);

        var table = header.Elements.OfType<Table>().Single();
        Assert.Single(table.Rows);
    }

    // -------------------------------------------------------------------------
    // Left cell — text content
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_LeftCell_ContainsTitleText()
    {
        var header = CreateHeader();
        new HeaderComponent(null).Render(header);

        var leftCell = header.Elements.OfType<Table>().Single().Rows[0].Cells[0];
        var paragraphTexts = leftCell.Elements.OfType<Paragraph>()
            .Select(p => p.GetRawText())
            .ToList();

        Assert.Contains(paragraphTexts, t => t.Contains("Wheelchair Configuration"));
    }

    [Fact]
    public void Render_LeftCell_ContainsSubtitleText()
    {
        var header = CreateHeader();
        new HeaderComponent(null).Render(header);

        var leftCell = header.Elements.OfType<Table>().Single().Rows[0].Cells[0];
        var paragraphTexts = leftCell.Elements.OfType<Paragraph>()
            .Select(p => p.GetRawText())
            .ToList();

        Assert.Contains(paragraphTexts, t => t.Contains("Vygenerováno systémem"));
    }

    [Fact]
    public void Render_TitleParagraph_HasBoldFont()
    {
        var header = CreateHeader();
        new HeaderComponent(null).Render(header);

        var leftCell = header.Elements.OfType<Table>().Single().Rows[0].Cells[0];
        var titleParagraph = leftCell.Elements.OfType<Paragraph>().First();
        Assert.True(titleParagraph.Format.Font.Bold);
    }

    [Fact]
    public void Render_TitleParagraph_HasExpectedFontSize()
    {
        var header = CreateHeader();
        new HeaderComponent(null).Render(header);

        var leftCell = header.Elements.OfType<Table>().Single().Rows[0].Cells[0];
        var titleParagraph = leftCell.Elements.OfType<Paragraph>().First();
        Assert.Equal(18, titleParagraph.Format.Font.Size.Point);
    }

    // -------------------------------------------------------------------------
    // Right cell — logo handling
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_WithNullLogo_RightCellHasNoParagraphWithImage()
    {
        var header = CreateHeader();
        new HeaderComponent(null).Render(header);

        var rightCell = header.Elements.OfType<Table>().Single().Rows[0].Cells[1];
        var hasImage = rightCell.Elements.OfType<Paragraph>()
            .SelectMany(p => p.Elements.OfType<Image>())
            .Any();

        Assert.False(hasImage);
    }

    [Fact]
    public void Render_WithEmptyLogoArray_RightCellHasNoParagraphWithImage()
    {
        var header = CreateHeader();
        new HeaderComponent([]).Render(header);

        var rightCell = header.Elements.OfType<Table>().Single().Rows[0].Cells[1];
        var hasImage = rightCell.Elements.OfType<Paragraph>()
            .SelectMany(p => p.Elements.OfType<Image>())
            .Any();

        Assert.False(hasImage);
    }

    [Fact]
    public void Render_WithValidLogo_RightCellContainsImage()
    {
        var header = CreateHeader();
        new HeaderComponent(CreateMinimalPngBytes()).Render(header);

        var rightCell = header.Elements.OfType<Table>().Single().Rows[0].Cells[1];
        var image = rightCell.Elements.OfType<Paragraph>()
            .SelectMany(p => p.Elements.OfType<Image>())
            .FirstOrDefault();

        Assert.NotNull(image);
    }

    [Fact]
    public void Render_WithValidLogo_ImageSourceContainsBase64Prefix()
    {
        var header = CreateHeader();
        new HeaderComponent(CreateMinimalPngBytes()).Render(header);

        var rightCell = header.Elements.OfType<Table>().Single().Rows[0].Cells[1];
        var image = rightCell.Elements.OfType<Paragraph>()
            .SelectMany(p => p.Elements.OfType<Image>())
            .First();

        Assert.StartsWith("base64:", image.Name);
    }

    [Fact]
    public void Render_WithValidLogo_ImageHasLockedAspectRatio()
    {
        var header = CreateHeader();
        new HeaderComponent(CreateMinimalPngBytes()).Render(header);

        var rightCell = header.Elements.OfType<Table>().Single().Rows[0].Cells[1];
        var image = rightCell.Elements.OfType<Paragraph>()
            .SelectMany(p => p.Elements.OfType<Image>())
            .First();

        Assert.True(image.LockAspectRatio);
    }

    [Fact]
    public void Render_WithValidLogo_ImageWidthIs2Point5Cm()
    {
        var header = CreateHeader();
        new HeaderComponent(CreateMinimalPngBytes()).Render(header);

        var rightCell = header.Elements.OfType<Table>().Single().Rows[0].Cells[1];
        var image = rightCell.Elements.OfType<Paragraph>()
            .SelectMany(p => p.Elements.OfType<Image>())
            .First();

        Assert.Equal(Unit.FromCentimeter(2.5), image.Width);
    }
}