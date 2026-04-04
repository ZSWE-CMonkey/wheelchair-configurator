// Soubor: Export/IExportFileBuilder.cs
using WheelchairConfigurator.Export.ExportModel;

namespace WheelchairConfigurator.Export;

/// <summary>
/// Defines the contract for any export file builder (PDF, CSV, etc.).
/// </summary>
public interface IExportFileBuilder
{
    string Build(ConfigurationExportModel model);
}