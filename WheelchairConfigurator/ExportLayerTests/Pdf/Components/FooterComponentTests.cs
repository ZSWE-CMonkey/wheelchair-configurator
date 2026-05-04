using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using WheelchairConfigurator.Export.Pdf.Components;
using WheelchairConfigurator.Export.Pdf;
using Xunit;

namespace WheelchairConfigurator.Export.Tests.Pdf.Components;

public class FooterComponentTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static HeaderFooter CreateFooter()
    {
        var document = new Document();
        var section = document.AddSection();
        return section.Footers.Primary;
    }

    // -------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_AddsExactlyOneParagraph()
    {
        var footer = CreateFooter();
        new FooterComponent().Render(footer);

        Assert.Single(footer.Elements.OfType<Paragraph>());
    }

    [Fact]
    public void Render_Paragraph_IsCenterAligned()
    {
        var footer = CreateFooter();
        new FooterComponent().Render(footer);

        var paragraph = footer.Elements.OfType<Paragraph>().Single();
        Assert.Equal(ParagraphAlignment.Center, paragraph.Format.Alignment);
    }

    [Fact]
    public void Render_Paragraph_HasFontSizeNine()
    {
        var footer = CreateFooter();
        new FooterComponent().Render(footer);

        var paragraph = footer.Elements.OfType<Paragraph>().Single();
        Assert.Equal(9, paragraph.Format.Font.Size.Point);
    }

    [Fact]
    public void Render_Paragraph_ContainsGeneratedOnText()
    {
        var footer = CreateFooter();
        new FooterComponent().Render(footer);

        var paragraph = footer.Elements.OfType<Paragraph>().Single();
        var texts = paragraph.Elements.OfType<Text>().Select(t => t.Content).ToList();

        Assert.Contains(texts, t => t.StartsWith("Generated on"));
    }

    [Fact]
    public void Render_Paragraph_ContainsPageFieldAndNumPagesField()
    {
        var footer = CreateFooter();
        new FooterComponent().Render(footer);

        var paragraph = footer.Elements.OfType<Paragraph>().Single();

        // MigraDoc represents §page and §numpages as specific element types
        Assert.Contains(paragraph.Elements, e => e is PageField);
        Assert.Contains(paragraph.Elements, e => e is NumPagesField);
    }

    [Fact]
    public void Render_Paragraph_HasPositiveSpaceBefore()
    {
        var footer = CreateFooter();
        new FooterComponent().Render(footer);

        var paragraph = footer.Elements.OfType<Paragraph>().Single();
        Assert.True(paragraph.Format.SpaceBefore.Point > 0);
    }

    [Fact]
    public void Render_Paragraph_FontColorMatchesGreyMedium()
    {
        var footer = CreateFooter();
        new FooterComponent().Render(footer);

        var paragraph = footer.Elements.OfType<Paragraph>().Single();
        Assert.Equal(PdfDocumentColors.GreyMedium, paragraph.Format.Font.Color);
    }
}