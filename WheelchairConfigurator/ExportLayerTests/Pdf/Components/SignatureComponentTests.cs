using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using WheelchairConfigurator.Export.Pdf.Components;
using WheelchairConfigurator.Export.Pdf;
using WheelchairConfigurator.Export.Tests;
using Xunit;

namespace WheelchairConfigurator.Export.Tests.Pdf.Components;

public class SignatureComponentTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Section CreateSection()
    {
        var document = new Document();
        return document.AddSection();
    }

    // -------------------------------------------------------------------------
    // Default title
    // -------------------------------------------------------------------------

    [Fact]
    public void Ctor_DefaultTitle_IsPodpis()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var table = section.Elements.OfType<Table>().Single();
        var labelRow = table.Rows[1];
        var labelText = labelRow.Cells[1].Elements.OfType<Paragraph>()
            .First().GetRawText();

        Assert.Equal("Podpis", labelText);
    }

    // -------------------------------------------------------------------------
    // Custom title
    // -------------------------------------------------------------------------

    [Fact]
    public void Ctor_CustomTitle_IsRenderedInLabelRow()
    {
        var section = CreateSection();
        new SignatureComponent("Podpis odpovědné osoby").Render(section);

        var table = section.Elements.OfType<Table>().Single();
        var labelRow = table.Rows[1];
        var labelText = labelRow.Cells[1].Elements.OfType<Paragraph>()
            .First().GetRawText();

        Assert.Equal("Podpis odpovědné osoby", labelText);
    }

    // -------------------------------------------------------------------------
    // Layout structure
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_AddsSpacerParagraphBeforeTable()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        // First element must be the spacer paragraph
        Assert.IsType<Paragraph>(section.Elements[0]);
    }

    [Fact]
    public void Render_SpacerParagraph_HasPositiveSpaceBefore()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var spacer = (Paragraph)section.Elements[0];
        Assert.True(spacer.Format.SpaceBefore.Point > 0);
    }

    [Fact]
    public void Render_AddsTableAfterSpacer()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        Assert.IsType<Table>(section.Elements[1]);
    }

    [Fact]
    public void Render_Table_HasTwoColumns()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var table = section.Elements.OfType<Table>().Single();
        Assert.Equal(2, table.Columns.Count);
    }

    [Fact]
    public void Render_Table_HasTwoRows()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var table = section.Elements.OfType<Table>().Single();
        Assert.Equal(2, table.Rows.Count);
    }

    [Fact]
    public void Render_TableBordersAreNotVisible()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var table = section.Elements.OfType<Table>().Single();
        Assert.False(table.Borders.Visible);
    }

    // -------------------------------------------------------------------------
    // Row 1 — date/place placeholders + signature line
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_FirstRow_LeftCell_ContainsDotPlaceholders()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var firstRow = section.Elements.OfType<Table>().Single().Rows[0];
        var leftText = firstRow.Cells[0].Elements.OfType<Paragraph>().First().GetRawText();

        Assert.Contains(".....", leftText);
        Assert.Contains("V ", leftText);
        Assert.Contains("dne", leftText);
    }

    [Fact]
    public void Render_FirstRow_RightCell_ContainsSignatureDots()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var firstRow = section.Elements.OfType<Table>().Single().Rows[0];
        var rightText = firstRow.Cells[1].Elements.OfType<Paragraph>().First().GetRawText();

        Assert.Contains("........", rightText);
    }

    [Fact]
    public void Render_FirstRow_RightCell_IsCenterAligned()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var firstRow = section.Elements.OfType<Table>().Single().Rows[0];
        var rightParagraph = firstRow.Cells[1].Elements.OfType<Paragraph>().First();

        Assert.Equal(ParagraphAlignment.Center, rightParagraph.Format.Alignment);
    }

    // -------------------------------------------------------------------------
    // Row 2 — label
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_SecondRow_LabelCell_IsCenterAligned()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var labelRow = section.Elements.OfType<Table>().Single().Rows[1];
        Assert.Equal(ParagraphAlignment.Center, labelRow.Cells[1].Format.Alignment);
    }

    [Fact]
    public void Render_SecondRow_LabelCell_FontSizeIsNine()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var labelRow = section.Elements.OfType<Table>().Single().Rows[1];
        var formattedText = labelRow.Cells[1].Elements.OfType<Paragraph>()
            .First().Elements.OfType<FormattedText>().First();

        Assert.Equal(9, formattedText.Font.Size.Point);
    }

    [Fact]
    public void Render_SecondRow_LabelCell_FontColorMatchesGreyDark()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var labelRow = section.Elements.OfType<Table>().Single().Rows[1];
        var formattedText = labelRow.Cells[1].Elements.OfType<Paragraph>()
            .First().Elements.OfType<FormattedText>().First();

        Assert.Equal(PdfDocumentColors.GreyDark, formattedText.Font.Color);
    }

    [Fact]
    public void Render_SecondRow_HasPositiveTopPadding()
    {
        var section = CreateSection();
        new SignatureComponent().Render(section);

        var labelRow = section.Elements.OfType<Table>().Single().Rows[1];
        Assert.True(labelRow.TopPadding.Point > 0);
    }
}