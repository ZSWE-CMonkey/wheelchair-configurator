using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using WheelchairConfigurator.Export.ExportModel;
using WheelchairConfigurator.Export.Pdf.Interfaces;

namespace WheelchairConfigurator.Export.Pdf.Components;

/// <summary>
/// PDF component responsible for rendering a table of configuration items along with their details and total price.
/// Features automatic text truncation to prevent layout breaking and zebra striping for better readability.
/// </summary>
public class ConfigurationTableComponent : IPdfComponent
{
    // Column widths definitions
    // Calculated based on a standard A4 page width with margins.
    private static readonly Unit ItemCategory = Unit.FromCentimeter(3.38);
    private static readonly Unit ItemName = Unit.FromCentimeter(5.07);
    private static readonly Unit ItemCode = Unit.FromCentimeter(3.38);
    private static readonly Unit Qty = Unit.FromCentimeter(1.79);
    private static readonly Unit Price = Unit.FromCentimeter(3.38);

    /// <summary>
    /// Maximum character lengths for category and name fields to ensure the table layout remains intact. Longer text will be truncated with an ellipsis.
    /// </summary>
    private const int MaxCategoryLength = 25;
    private const int MaxNameLength = 40;

    private readonly ConfigurationExportModel _model;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationTableComponent"/> class with the provided
    /// configuration export model containing the items to be displayed in the table.
    /// </summary> <param name="model">The configuration export model with item details.</param>
    public ConfigurationTableComponent(ConfigurationExportModel model)
    {
        _model = model;
    }

    /// <summary>
    /// Renders the configuration table and the total price summary into the provided document section.
    /// </summary>
    /// <param name="section">The MigraDoc section where the content will be added.</param>
    public void Render(Section section)
    {
        var table = BuildTable(section);
        AddHeaderRow(table);
        AddDataRows(table);
        AddTotalPrice(section);
    }

    /// <summary>
    /// Initializes the MigraDoc table structure, disables default borders, and defines the columns.
    /// </summary>
    /// <param name="section">The parent document section.</param>
    /// <returns>The newly created <see cref="Table"/> instance.</returns>
    private static Table BuildTable(Section section)
    {
        var table = section.AddTable();
        table.Borders.Visible = false;
        table.Format.Font.Size = 12;

        table.AddColumn(ItemCategory);
        table.AddColumn(ItemName);
        table.AddColumn(ItemCode);
        table.AddColumn(Qty);
        table.AddColumn(Price);

        return table;
    }

    /// <summary>
    /// Adds the header row to the table with defined column titles and bottom borders.
    /// </summary>
    /// <param name="table">The table to which the header row will be added.</param>
    private static void AddHeaderRow(Table table)
    {
        var row = table.AddRow();
        row.BottomPadding = Unit.FromPoint(5);

        SetHeaderCell(row.Cells[0], "Kategorie", ParagraphAlignment.Left);
        SetHeaderCell(row.Cells[1], "Název komponenty", ParagraphAlignment.Left);
        SetHeaderCell(row.Cells[2], "Kód položky", ParagraphAlignment.Right);
        SetHeaderCell(row.Cells[3], "Ks", ParagraphAlignment.Right);
        SetHeaderCell(row.Cells[4], "Cena", ParagraphAlignment.Right);
    }

    /// <summary>
    /// Adds the data rows to the table with item details and alternating shading.
    /// </summary>
    /// <param name="table">The table to which the data rows will be added.</param>
    private void AddDataRows(Table table)
    {
        int index = 0;
        foreach (var item in _model.Items)
        {
            AddDataRow(table, item, isOdd: index % 2 != 0);
            index++;
        }
    }
    /// <summary>
    /// Adds a single data row representing a configuration item. 
    /// Applies zebra striping if the row is odd.
    /// </summary>
    /// <param name="table">The target table.</param>
    /// <param name="item">The configuration item data.</param>
    /// <param name="isOdd">Flag indicating whether the row index is odd (used for background shading).</param>
    private static void AddDataRow(Table table, ConfigurationExportItem item, bool isOdd)
    {
        var row = table.AddRow();
        row.TopPadding = Unit.FromPoint(5);
        row.BottomPadding = Unit.FromPoint(5);

        if (isOdd)
            row.Shading.Color = PdfDocumentColors.GreyLight;

        AddCell(row, 0, Truncate(item.CategoryName, MaxCategoryLength));
        AddCell(row, 1, Truncate(item.ComponentName, MaxNameLength));
        AddCell(row, 2, item.ItemCode, ParagraphAlignment.Right);
        AddCell(row, 3, item.Quantity.ToString(), ParagraphAlignment.Right);
        AddCell(row, 4, $"${item.Price:N2}", ParagraphAlignment.Right);
    }

    /// <summary>
    /// Adds a summary paragraph below the table displaying the total price of the configuration, aligned to the right and styled for emphasis.
    /// </summary>
    /// <param name="section">The parent document section.</param>
    private void AddTotalPrice(Section section)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.Alignment = ParagraphAlignment.Right;
        paragraph.Format.SpaceBefore = Unit.FromPoint(15);

        var label = paragraph.AddFormattedText("Celková cena: ");
        label.Font.Size = 16;

        var price = paragraph.AddFormattedText($"${_model.TotalPrice:N2}");
        price.Font.Size = 16;
        price.Font.Color = PdfDocumentColors.PrimaryBlue;
    }

    /// <summary>
    /// Helper method to configure a table header cell with specific formatting and bottom border.
    /// </summary>
    /// <param name="cell">The cell to format.</param>
    /// <param name="text">The header text.</param>
    /// <param name="alignment">The paragraph alignment.</param>
    private static void SetHeaderCell(Cell cell, string text, ParagraphAlignment alignment)
    {
        cell.Format.Alignment = alignment;
        cell.Borders.Bottom.Width = 0.5;
        cell.Borders.Bottom.Color = PdfDocumentColors.TableBorder;
        cell.AddParagraph(text);
    }

    /// <summary>
    /// Helper method to populate a standard data cell with text and alignment.
    /// </summary>
    /// <param name="row">The parent row.</param>
    /// <param name="index">The column index of the cell.</param>
    /// <param name="text">The content to insert.</param>
    /// <param name="alignment">The paragraph alignment (default is Left).</param>
    private static void AddCell(Row row, int index, string text,
        ParagraphAlignment alignment = ParagraphAlignment.Left)
    {
        row.Cells[index].Format.Alignment = alignment;
        row.Cells[index].AddParagraph(text);
    }

    /// <summary>
    /// Safely truncates a given string to the specified maximum length and appends an ellipsis (...) if truncated.
    /// </summary>
    /// <param name="text">The original text string.</param>
    /// <param name="maxLength">The maximum allowed number of characters.</param>
    /// <returns>The original or truncated string.</returns>
    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..maxLength] + "…";
}
