using WheelchairConfigurator.Export.ExportModel;
using WheelchairConfigurator.Data;

namespace WheelchairConfigurator.Export;

/// <summary>
/// Orchestrates the export pipeline.
/// Loads configuration data and routes to the correct builder based on format.
/// </summary>
public class ExportService : IExportService
{


    private readonly IExportFileBuilder _fileBuilder;
    private readonly DbService _dbService;

    public ExportService(IExportFileBuilder fileBuilder, DbService dbService)
    {
        _fileBuilder = fileBuilder;
        _dbService = dbService;
    }

    /// <summary>
    /// Exports the configuration to the requested format.
    /// Returns the file path of the generated file.
    /// </summary>
    public async Task<string> ExportAsync(int configurationId, ExportFormat format)
    {
        try
    {
        // TODO: Replace mock data with actual database calls to load configuration details.
        var exportData = GetMockData(configurationId);

        return format switch
        {
            ExportFormat.Pdf => _fileBuilder.Build(exportData),
            _ => throw new NotImplementedException($"Format {format} is not supported.")
        };
    }

           catch (Exception ex)
    {
        Console.WriteLine($"[ExportService] ERROR: Export failed — {ex.Message}");
        throw;
    }
    }

    /// <summary>
    /// Temporary mock data — will be replaced by repository calls.
    /// </summary>
    private ConfigurationExportModel GetMockData(int id)
    {
        return new ConfigurationExportModel
        {
            ConfigurationName = $"Test Configuration #{id}",
            SpecialistName = "Dr. House",
            CreatedAt = DateTime.Now,
            TotalPrice = 1500.50m,
            Items = new List<ConfigurationExportItem>
            {
                new() { CategoryName = "Frame", ComponentName = "Standard Aluminum Frame", Price = 500.00m, ItemCode = "FRM-ALU-STD" },
                new() { CategoryName = "Wheels", ComponentName = "Off-road Wheels 24\"", Price = 800.50m, ItemCode = "WHL-OFF-24" },
                new() { CategoryName = "Seat", ComponentName = "Ergonomic Cushion", Price = 200.00m, ItemCode = "SEAT-ERG" }
            }
        };
    }
}