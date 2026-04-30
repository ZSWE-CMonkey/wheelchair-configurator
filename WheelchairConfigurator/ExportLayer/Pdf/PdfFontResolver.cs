using PdfSharp.Fonts;

namespace WheelchairConfigurator.Export.Pdf;

/// <summary>
/// Custom font resolver that loads fonts from externally registered byte arrays.
/// Fonts must be registered by the host application (MAUI) before generating any PDF.
/// This ensures cross-platform compatibility — no dependency on OS fonts or embedded resources.
///
/// SETUP INSTRUCTIONS:
/// 1. In MauiProgram.cs, load font bytes and register them:
///    PdfFontResolver.Instance.RegisterFont("Roboto", await LoadFontBytesAsync("Roboto-Regular.ttf"));
///    GlobalFontSettings.FontResolver = PdfFontResolver.Instance;
/// </summary>
public sealed class PdfFontResolver : IFontResolver
{
    /// <summary>The singleton instance of the font resolver.</summary>
    public static readonly PdfFontResolver Instance = new();

    private readonly Dictionary<string, byte[]> _fonts = new(StringComparer.OrdinalIgnoreCase);

    private PdfFontResolver() { }

    /// <summary>
    /// Gets the default font name used when no specific font is requested.
    /// </summary>
    public string DefaultFontName => "Roboto";

    /// <summary>
    /// Registers a font by name so the resolver can provide it to PDFsharp.
    /// Must be called before generating the first PDF.
    /// </summary>
    /// <param name="fontName">The font family name (e.g. "Roboto").</param>
    /// <param name="fontBytes">The raw TTF file bytes.</param>
    public void RegisterFont(string fontName, byte[] fontBytes)
    {
        _fonts[fontName] = fontBytes;
    }

    /// <summary>
    /// Maps any font family request to our registered font.
    /// </summary>
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        return new FontResolverInfo("Roboto");
    }

    /// <summary>
    /// Returns the raw bytes of the requested font face.
    /// </summary>
    public byte[]? GetFont(string faceName)
    {
        // Tady už dynamicky hledáme podle toho, co nám přišlo z ResolveTypeface
        if (_fonts.TryGetValue(faceName, out var bytes))
            return bytes;

        throw new InvalidOperationException(
            $"Font '{faceName}' is not registered. " +
            "Call PdfFontResolver.Instance.RegisterFont() before generating the PDF.");
    }
}