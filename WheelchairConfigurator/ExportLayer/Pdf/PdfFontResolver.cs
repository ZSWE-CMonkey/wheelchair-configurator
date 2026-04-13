using PdfSharp.Fonts;

namespace WheelchairConfigurator.Export.Pdf;

/// <summary>
/// Custom font resolver that loads the Roboto font from the assembly's embedded resources.
/// This is essential for cross-platform compatibility (.NET MAUI / Android / iOS), as it 
/// eliminates the dependency on local operating system fonts which often cause runtime crashes.
/// 
/// SETUP INSTRUCTIONS:
/// 1. Add the font file to the project and mark it in the .csproj:
///    <ItemGroup>
///        <EmbeddedResource Include="Fonts\Roboto-Regular.ttf" />
///    </ItemGroup>
/// 2. Register the resolver before generating the first PDF:
///    GlobalFontSettings.FontResolver = PdfFontResolver.Instance;
/// </summary>
public sealed class PdfFontResolver : IFontResolver
{
    /// <summary>
    /// The singleton instance of the font resolver.
    /// </summary>
    public static readonly PdfFontResolver Instance = new();

    /// <summary>
    /// Prevents a default instance of the <see cref="PdfFontResolver"/> class from being created.
    /// </summary>
    private PdfFontResolver() { }

    /// <summary>
    /// Gets the default font name used by MigraDoc/PdfSharp when no specific font is defined.
    /// This should match the base name of your embedded font.
    /// </summary> 
    /// <value>The default font name (e.g., "Roboto").</value>
    public string DefaultFontName => "Roboto";

    /// <summary>
    /// Maps any font family request to our embedded custom font.
    /// By returning "Roboto#regular" for everything, we prevent crashes when the system 
    /// implicitly requests standard fonts like 'Arial' or 'Courier New'.
    /// </summary>
    /// <param name="familyName">The requested font family name.</param>
    /// <param name="isBold">Indicates whether a bold typeface was requested.</param>
    /// <param name="isItalic">Indicates whether an italic typeface was requested.</param>
    /// <returns>The resolved font information containing the internal face name.</returns>
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        return new FontResolverInfo("Roboto#regular");
    }

    /// <summary>
    /// Retrieves the byte array of the embedded font resource based on the provided face name.
    /// The face name is determined by the ResolveTypeface method and should correspond to the naming convention used in the embedded resources.
    /// If the requested face name does not match any known fonts, it defaults to returning the Roboto-Regular font to ensure that PDF generation can proceed without errors, albeit with a fallback.
    /// </summary>
    /// <param name="faceName">The internal face name resolved by <see cref="ResolveTypeface"/>.</param>
    /// <returns>A byte array containing the TTF file data.</returns>
    public byte[]? GetFont(string faceName)
    {
        return faceName switch
        {
            "Roboto#regular" => GetResourceBytes("Roboto-Regular.ttf"),
            _ => GetResourceBytes("Roboto-Regular.ttf")
        };
    }

    /// <summary>
    /// Reads the font file directly from the assembly's embedded resources into a byte array.
    /// </summary>
    /// <param name="fileName">The exact name of the file (e.g., 'Roboto-Regular.ttf').</param>
    /// <returns>A byte array containing the embedded resource data.</returns>
    /// <exception cref="Exception">Thrown when the requested file is not found within the embedded resources.</exception>
    public static byte[] GetResourceBytes(string fileName)
    {
        var assembly = typeof(PdfFontResolver).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new Exception($"Embedded font '{fileName}' not found. " +
                $"Make sure the file is added as EmbeddedResource in .csproj.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}