using WheelchairConfigurator.Export.ExportModel;

namespace WheelchairConfigurator.Export;

/// <summary>
/// Defines the contract for the export service.
/// Implement this interface to support additional export formats in the future.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports the configuration with the given ID to the requested format.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to export.</param>
    /// <param name="format">The desired output format (e.g. PDF).</param>
    /// <returns>The file path of the generated export file.</returns>
    Task<string> ExportAsync(int configurationId, ExportFormat format);
}