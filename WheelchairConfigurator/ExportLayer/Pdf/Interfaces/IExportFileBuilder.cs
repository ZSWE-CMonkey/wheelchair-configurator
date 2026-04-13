// Soubor: Export/IExportFileBuilder.cs
using WheelchairConfigurator.Export.ExportModel;

namespace WheelchairConfigurator.Export;
/// <summary>
/// Contract for building export files (e.g., PDF) from a given configuration export model.
/// </summary>
public interface IExportFileBuilder
{
    /// <summary>
    /// Builds an export file (e.g., PDF) from the provided configuration export model and returns it as a byte array.
    /// </summary>
    /// <param name="model">The configuration export model.</param>
    /// <returns>The export file as a byte array.</returns>
    byte[] Build(ConfigurationExportModel model);
}