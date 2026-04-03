using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Seeding.Seeders;

/// <summary>
/// Seeds the Component table and returns a name-to-ID map for downstream seeders.
/// </summary>
public class ComponentSeeder
{
    /// <summary>
    /// Inserts all components from the DTO, resolving category names to IDs via categoryMap.
    /// Returns a dictionary mapping component name to its generated database ID.
    /// </summary>
    public Dictionary<string, int> Seed(SQLiteConnection db, List<ComponentDto> dtos, Dictionary<string, int> categoryMap)
    {
        var map = new Dictionary<string, int>();

        foreach (var dto in dtos)
        {
            if (!categoryMap.TryGetValue(dto.CategoryName, out int categoryId))
            {
                Console.WriteLine($"[ComponentSeeder] SKIP '{dto.Name}' — category '{dto.CategoryName}' not found.");
                continue;
            }

            var entity = new Component
            {
                Name = dto.Name,
                CategoryId = categoryId,
                CatalogUrl = dto.CatalogUrl,
                Price = dto.Price
            };
            db.Insert(entity);
            map[dto.Name] = entity.Id;
        }

        Console.WriteLine($"[ComponentSeeder] Components: {map.Count}");
        return map;
    }
}