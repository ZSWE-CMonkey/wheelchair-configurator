using MigraDoc.DocumentObjectModel;

namespace WheelchairConfigurator.Export.Pdf.Interfaces;

/// <summary>
/// Contract for a PDF footer component that can render itself into a given MigraDoc footer section.
/// </summary>
public interface IPdfFooterComponent
{
    /// <summary>
    /// Renders the PDF footer component into the provided MigraDoc footer section.
    /// </summary>
    /// <param name="footer">The MigraDoc footer section into which the component should be rendered.</param>
    void Render(HeaderFooter footer);
}
