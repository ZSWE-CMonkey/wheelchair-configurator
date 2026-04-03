using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Seeding.Seeders;

/// <summary>
/// Seeds the Category table and returns a name-to-ID map for downstream seeders.
/// </summary>
public class CategorySeeder
{
    /// <summary>
    /// Inserts all categories from the DTO and returns a dictionary mapping
    /// category name to its generated database ID.
    /// </summary>
    public Dictionary<string, int> Seed(SQLiteConnection db, List<CategoryDto> dtos)
    {
        var map = new Dictionary<string, int>();

        foreach (var dto in dtos)
        {
            var entity = new Category { Name = dto.Name };
            db.Insert(entity);
            map[dto.Name] = entity.Id;
        }

        Console.WriteLine($"[CategorySeeder] Categories: {map.Count}");
        return map;
    }
}