using MigraDoc.DocumentObjectModel;

namespace WheelchairConfigurator.Export.Pdf;

/// <summary>
/// Centralized color palette used throughout the PDF document generation.
/// Defines standard brand colors, text shades, and UI element colors to ensure visual consistency.
/// </summary>
internal static class PdfDocumentColors
{
    /// <summary>
    /// The primary brand color, typically used for highlights, main titles, and emphasizing the total price.
    /// </summary>
    public static readonly MigraDoc.DocumentObjectModel.Color PrimaryBlue = MigraDoc.DocumentObjectModel.Color.FromRgb(21, 101, 192);

    /// <summary>
    /// Medium grey color, suitable for secondary text such as footers, page numbers, and timestamps.
    /// </summary>
    public static readonly MigraDoc.DocumentObjectModel.Color GreyMedium = MigraDoc.DocumentObjectModel.Color.FromRgb(158, 158, 158);

    /// <summary>
    /// Light grey color, primarily used as a background color for zebra striping in data tables.
    /// </summary>
    public static readonly MigraDoc.DocumentObjectModel.Color GreyLight = MigraDoc.DocumentObjectModel.Color.FromRgb(245, 245, 245);

    /// <summary>
    /// Dark grey color, used for subtitles, input labels, and less prominent text elements.
    /// </summary>
    public static readonly MigraDoc.DocumentObjectModel.Color GreyDark = MigraDoc.DocumentObjectModel.Color.FromRgb(97, 97, 97);

    /// <summary>
    /// Standard border color used for separating rows and defining table boundaries.
    /// </summary>
    public static readonly MigraDoc.DocumentObjectModel.Color TableBorder = MigraDoc.DocumentObjectModel.Color.FromRgb(189, 189, 189);
}
