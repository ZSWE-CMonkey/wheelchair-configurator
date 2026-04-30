using MigraDoc.DocumentObjectModel;

namespace WheelchairConfigurator.Export.Pdf.Interfaces;

/// <summary>
/// Contract for a PDF header component that can render itself into a given MigraDoc header section.
/// </summary>
public interface IPdfHeaderComponent
{
    /// <summary>
    /// Renders the PDF header component into the provided MigraDoc header section.
    /// </summary>
    /// <param name="header">The MigraDoc header section into which the component should be rendered.</param>
    void Render(HeaderFooter header);
}
