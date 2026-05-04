using WheelchairConfigurator.Data;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Data.Providers;

namespace WheelchairConfigurator.Service;

/// <summary>
/// Orchestrates the data processing pipeline.
/// Retrieves seed file paths and delegates parsing to <see cref="JsonDataLoader"/>.
/// </summary>
public class DataService
{
    private readonly ILocalFileProvider _fileProvider;
    private readonly JsonDataLoader _loader;

    public DataService(ILocalFileProvider fileProvider, JsonDataLoader loader)
    {
        _fileProvider = fileProvider;
        _loader = loader;
    }

    /// <summary>
    /// Runs the pipeline: resolves file paths, parses each file into a <see cref="SeedDataDto"/>.
    /// </summary>
    /// <returns>List of successfully loaded seed data objects.</returns>
    public List<SeedDataDto> ProcessData()
    {
        Console.WriteLine("[DataService] Starting data processing pipeline...");

        var result = new List<SeedDataDto>();
        string[] paths = _fileProvider.GetSeedFilePaths();

        if (paths.Length == 0)
        {
            Console.WriteLine("[DataService] No seed files found.");
            return result;
        }

        foreach (var path in paths)
        {
            var dto = _loader.LoadData(path);

            if (dto is not null)
            {
                result.Add(dto);
                Console.WriteLine($"[DataService] OK: {dto.Categories.Count} categories, " +
                                  $"{dto.Components.Count} components loaded from '{path}'.");
            }
            else
            {
                Console.WriteLine($"[DataService] SKIP: Failed to load '{path}'.");
            }
        }

        Console.WriteLine("[DataService] Pipeline complete.");
        return result;
    }
}