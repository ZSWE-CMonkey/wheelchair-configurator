using System.Text.Json;
using WheelchairConfigurator.Data.DTOs;

namespace WheelchairConfigurator.Data;

/// <summary>
/// Loads and parses JSON seed files into DTOs.
/// Does not interact with the database.
/// </summary>
public class JsonDataLoader
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Reads a JSON file from the given path and deserializes it into a <see cref="SeedDataDto"/>.
    /// </summary>
    /// <param name="filePath">Relative or absolute path to the JSON file.</param>
    /// <returns>Populated <see cref="SeedDataDto"/>, or null on failure.</returns>
    public SeedDataDto? LoadData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[JsonDataLoader] ERROR: File '{filePath}' not found.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            var result = JsonSerializer.Deserialize<SeedDataDto>(json, _options);
            Console.WriteLine($"[JsonDataLoader] OK: '{filePath}' loaded successfully.");
            return result;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[JsonDataLoader] ERROR: JSON parse failed — {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JsonDataLoader] ERROR: Unexpected error — {ex.Message}");
            return null;
        }
    }
}