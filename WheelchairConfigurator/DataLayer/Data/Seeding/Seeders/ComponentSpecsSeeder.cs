using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Seeding.Seeders;

/// <summary>
/// Seeds the ComponentSpecs table.
/// </summary>
public class ComponentSpecsSeeder
{
    /// <summary>
    /// Inserts all component specs, resolving component names to IDs via componentMap.
    /// </summary>
    public void Seed(SQLiteConnection db, List<ComponentSpecsDto> dtos, Dictionary<string, int> componentMap)
    {
        int count = 0;

        foreach (var dto in dtos)
        {
            if (!componentMap.TryGetValue(dto.ComponentName, out int compId))
            {
                Console.WriteLine($"[ComponentSpecsSeeder] SKIP spec — component '{dto.ComponentName}' not found.");
                continue;
            }

            db.Insert(new ComponentSpecs
            {
                ComponentId = compId,
                WeightCapacityKg = dto.WeightCapacityKg,
                SeatWidthCm = dto.SeatWidthCm,
                MaxSpeedKmh = dto.MaxSpeedKmh
            });
            count++;
        }

        Console.WriteLine($"[ComponentSpecsSeeder] Specs: {count}");
    }
}