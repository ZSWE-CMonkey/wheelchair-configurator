using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Seeding.Seeders;

/// <summary>
/// Seeds the CompatibilityRule table.
/// </summary>
public class CompatibilityRuleSeeder
{
    /// <summary>
    /// Inserts all compatibility rules, resolving component names to IDs via componentMap.
    /// </summary>
    public void Seed(SQLiteConnection db, List<CompatibilityRuleDto> dtos, Dictionary<string, int> componentMap)
    {
        int count = 0;

        foreach (var dto in dtos)
        {
            if (!componentMap.TryGetValue(dto.ComponentAName, out int compAId) ||
                !componentMap.TryGetValue(dto.ComponentBName, out int compBId))
            {
                Console.WriteLine($"[CompatibilityRuleSeeder] SKIP rule '{dto.ComponentAName}' ↔ '{dto.ComponentBName}' — component not found.");
                continue;
            }

            db.Insert(new CompatibilityRule
            {
                ComponentAId = compAId,
                ComponentBId = compBId,
                IsCompatible = dto.IsCompatible
            });
            count++;
        }

        Console.WriteLine($"[CompatibilityRuleSeeder] Compatibility rules: {count}");
    }
}