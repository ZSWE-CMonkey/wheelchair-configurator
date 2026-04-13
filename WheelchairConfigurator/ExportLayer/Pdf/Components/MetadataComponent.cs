using MigraDoc.DocumentObjectModel;
using WheelchairConfigurator.Export.ExportModel;
using WheelchairConfigurator.Export.Pdf.Interfaces;

namespace WheelchairConfigurator.Export.Pdf.Components;

/// <summary>
/// PDF component responsible for rendering the metadata section of the configuration document.
/// Displays key information such as the configuration name, assigned specialist, and creation date.
/// </summary>
public class MetadataComponent : IPdfComponent
{
    private readonly ConfigurationExportModel _model;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataComponent"/> class.
    /// </summary>
    /// <param name="model">The export model containing the metadata to be displayed.</param>
    public MetadataComponent(ConfigurationExportModel model)
    {
        _model = model;
    }

    /// <summary>
    /// Renders the metadata information into the provided document section.
    /// </summary>
    /// <param name="section">The MigraDoc section where the content will be added.</param>
    public void Render(Section section)
    {
        AddRow(section, "Konfigurace: ", _model.ConfigurationName, spaceBefore: 15);
        AddRow(section, "Specialista: ", _model.SpecialistName);
        AddRow(section, "Vytvořeno: ", _model.CreatedAt.ToString("g"), spaceAfter: 15);
    }

    /// <summary>
    /// Helper method to format and insert a single row of metadata with customizable vertical spacing.
    /// </summary>
    /// <param name="section">The parent document section.</param>
    /// <param name="label">The descriptive label for the metadata field (e.g., 'Konfigurace: ').</param>
    /// <param name="value">The actual value to display.</param>
    /// <param name="spaceBefore">The space above the paragraph in points (default is 0).</param>
    /// <param name="spaceAfter">The space below the paragraph in points (default is 5).</param>
    private static void AddRow(
        Section section,
        string label,
        string value,
        double spaceBefore = 0,
        double spaceAfter = 5)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.SpaceBefore = Unit.FromPoint(spaceBefore);
        paragraph.Format.SpaceAfter = Unit.FromPoint(spaceAfter);

        paragraph.AddText(label);
        paragraph.AddText(value);
    }
}
