using MigraDoc.DocumentObjectModel;

namespace WheelchairConfigurator.Export.Pdf.Interfaces;

/// <summary>
/// Contract for a core PDF component that can render itself into a given MigraDoc section.
/// </summary>
public interface IPdfComponent
{
    /// <summary>
    /// Renders the component's content into the provided MigraDoc section.
    /// </summary>
    /// <param name="section">The MigraDoc section where the content will be added.</param>
    void Render(Section section);
}