using System.Collections.Generic;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using WheelchairConfigurator.Export.ExportModel;
using WheelchairConfigurator.Export.Pdf.Components;
using WheelchairConfigurator.Export.Pdf;
using WheelchairConfigurator.Export.Tests; // Odkaz na tvůj soubor MigraDocExtensions
using Xunit;

namespace WheelchairConfigurator.Export.Tests.Pdf.Components;

public class ConfigurationTableComponentTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Section CreateSection()
    {
        var document = new Document();
        return document.AddSection();
    }

    private static ConfigurationExportModel CreateModel(
        IEnumerable<ConfigurationExportItem>? items = null,
        decimal totalPrice = 0m)
    {
        return new ConfigurationExportModel
        {
            Items = items?.ToList() ?? [],
            TotalPrice = totalPrice
        };
    }

    private static ConfigurationExportItem CreateItem(
        string category = "Category",
        string name = "Component Name",
        string code = "CODE-001",
        int qty = 1,
        decimal price = 99.99m)
    {
        return new ConfigurationExportItem
        {
            CategoryName = category,
            ComponentName = name,
            ItemCode = code,
            Quantity = qty,
            Price = price
        };
    }

    // -------------------------------------------------------------------------
    // Table structure
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_EmptyModel_AddsTableWithHeaderRowOnly()
    {
        var section = CreateSection();
        var component = new ConfigurationTableComponent(CreateModel());

        component.Render(section);

        var table = section.Elements.OfType<Table>().Single();
        // Header row only → 1 row
        Assert.Single(table.Rows);
    }

    [Fact]
    public void Render_SingleItem_AddsHeaderRowPlusOneDataRow()
    {
        var section = CreateSection();
        var model = CreateModel(items: [CreateItem()]);
        new ConfigurationTableComponent(model).Render(section);

        var table = section.Elements.OfType<Table>().Single();
        Assert.Equal(2, table.Rows.Count);
    }

    [Fact]
    public void Render_MultipleItems_RowCountMatchesItemCountPlusOne()
    {
        var items = Enumerable.Range(0, 5).Select(_ => CreateItem()).ToList();
        var section = CreateSection();
        new ConfigurationTableComponent(CreateModel(items: items)).Render(section);

        var table = section.Elements.OfType<Table>().Single();
        Assert.Equal(6, table.Rows.Count); // 1 header + 5 data rows
    }

    [Fact]
    public void Render_TableHasFiveColumns()
    {
        var section = CreateSection();
        new ConfigurationTableComponent(CreateModel()).Render(section);

        var table = section.Elements.OfType<Table>().Single();
        Assert.Equal(5, table.Columns.Count);
    }

    // -------------------------------------------------------------------------
    // Header row content
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_HeaderRow_ContainsExpectedColumnTitles()
    {
        var section = CreateSection();
        new ConfigurationTableComponent(CreateModel()).Render(section);

        var headerRow = section.Elements.OfType<Table>().Single().Rows[0];
        Assert.Equal("Kategorie", headerRow.Cells[0].Elements.OfType<Paragraph>().First().GetRawText());
        Assert.Equal("Název komponenty", headerRow.Cells[1].Elements.OfType<Paragraph>().First().GetRawText());
        Assert.Equal("Kód položky", headerRow.Cells[2].Elements.OfType<Paragraph>().First().GetRawText());
        Assert.Equal("Ks", headerRow.Cells[3].Elements.OfType<Paragraph>().First().GetRawText());
        Assert.Equal("Cena", headerRow.Cells[4].Elements.OfType<Paragraph>().First().GetRawText());
    }

    // -------------------------------------------------------------------------
    // Data row content
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_DataRow_ContainsCorrectItemValues()
    {
        var item = CreateItem(category: "Cat", name: "Comp", code: "X-1", qty: 3, price: 50m);
        var section = CreateSection();
        new ConfigurationTableComponent(CreateModel(items: [item])).Render(section);

        var dataRow = section.Elements.OfType<Table>().Single().Rows[1];
        Assert.Equal("Cat", dataRow.Cells[0].Elements.OfType<Paragraph>().First().GetRawText());
        Assert.Equal("Comp", dataRow.Cells[1].Elements.OfType<Paragraph>().First().GetRawText());
        Assert.Equal("X-1", dataRow.Cells[2].Elements.OfType<Paragraph>().First().GetRawText());
        Assert.Equal("3", dataRow.Cells[3].Elements.OfType<Paragraph>().First().GetRawText());

        var priceText = dataRow.Cells[4].Elements.OfType<Paragraph>().First().GetRawText();
        Assert.Contains("50", priceText);
    }

    // -------------------------------------------------------------------------
    // Zebra striping (shading)
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_EvenIndexRows_HaveNoShading()
    {
        var items = Enumerable.Range(0, 3).Select(_ => CreateItem()).ToList();
        var section = CreateSection();
        new ConfigurationTableComponent(CreateModel(items: items)).Render(section);

        var table = section.Elements.OfType<Table>().Single();
        // Row index 0 in data = table row index 1 -> even row has no shading
        var evenRow = table.Rows[1];
        Assert.True(evenRow.Shading.Color.IsEmpty);
    }

    [Fact]
    public void Render_OddIndexRows_HaveGreyShading()
    {
        var items = Enumerable.Range(0, 3).Select(_ => CreateItem()).ToList();
        var section = CreateSection();
        new ConfigurationTableComponent(CreateModel(items: items)).Render(section);

        var table = section.Elements.OfType<Table>().Single();
        // Row index 1 in data = table row index 2 -> odd row is shaded
        var oddRow = table.Rows[2];
        Assert.Equal(PdfDocumentColors.GreyLight, oddRow.Shading.Color);
    }

    // -------------------------------------------------------------------------
    // Text truncation
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(25, false)]  // exactly at limit -> no truncation
    [InlineData(24, false)]  // below limit -> no truncation
    [InlineData(26, true)]   // over limit -> truncated
    public void Render_CategoryName_TruncatedWhenExceedsMaxLength(int length, bool expectTruncation)
    {
        var longCategory = new string('A', length);
        var item = CreateItem(category: longCategory);
        var section = CreateSection();
        new ConfigurationTableComponent(CreateModel(items: [item])).Render(section);

        var cellText = section.Elements.OfType<Table>().Single().Rows[1]
            .Cells[0].Elements.OfType<Paragraph>().First().GetRawText();

        if (expectTruncation)
        {
            Assert.EndsWith("…", cellText);
            Assert.True(cellText.Length <= 26);
        }
        else
        {
            Assert.Equal(longCategory, cellText);
        }
    }

    [Theory]
    [InlineData(40, false)]  // exactly at limit -> no truncation
    [InlineData(39, false)]  // below limit -> no truncation
    [InlineData(41, true)]   // over limit -> truncated
    public void Render_ComponentName_TruncatedWhenExceedsMaxLength(int length, bool expectTruncation)
    {
        var longName = new string('B', length);
        var item = CreateItem(name: longName);
        var section = CreateSection();
        new ConfigurationTableComponent(CreateModel(items: [item])).Render(section);

        var cellText = section.Elements.OfType<Table>().Single().Rows[1]
            .Cells[1].Elements.OfType<Paragraph>().First().GetRawText();

        if (expectTruncation)
        {
            Assert.EndsWith("…", cellText);
            Assert.True(cellText.Length <= 41);
        }
        else
        {
            Assert.Equal(longName, cellText);
        }
    }

    // -------------------------------------------------------------------------
    // Total price paragraph
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_TotalPriceParagraph_IsAddedAfterTable()
    {
        var section = CreateSection();
        new ConfigurationTableComponent(CreateModel(totalPrice: 1234.56m)).Render(section);

        var lastElement = section.Elements[^1];
        Assert.IsType<Paragraph>(lastElement);
    }

    [Fact]
    public void Render_TotalPriceParagraph_ContainsTotalPriceValue()
    {
        var section = CreateSection();
        new ConfigurationTableComponent(CreateModel(totalPrice: 1234.56m)).Render(section);

        var paragraph = (Paragraph)section.Elements[^1];
        var rawText = paragraph.GetRawText();
        var cleanText = rawText.Replace(" ", "").Replace("\u00a0", "").Replace(".", ",");

        Assert.Contains("1234,56", cleanText);
    }

    [Fact]
    public void Render_TotalPriceParagraph_IsRightAligned()
    {
        var section = CreateSection();
        new ConfigurationTableComponent(CreateModel()).Render(section);

        var paragraph = (Paragraph)section.Elements[^1];
        Assert.Equal(ParagraphAlignment.Right, paragraph.Format.Alignment);
    }
}