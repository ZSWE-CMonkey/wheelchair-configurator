using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Seeding.Seeders;

/// <summary>
/// Seeds the Model3D table.
/// </summary>
public class Model3DSeeder
{
    /// <summary>
    /// Inserts all 3D models, resolving component names to IDs via componentMap.
    /// </summary>
    public void Seed(SQLiteConnection db, List<Model3DDto> dtos, Dictionary<string, int> componentMap)
    {
        int count = 0;

        foreach (var dto in dtos)
        {
            if (!componentMap.TryGetValue(dto.ComponentName, out int compId))
            {
                Console.WriteLine($"[Model3DSeeder] SKIP model — component '{dto.ComponentName}' not found.");
                continue;
            }

            db.Insert(new Model3D
            {
                ComponentId = compId,
                FilePath = dto.FilePath,
                TextureId = dto.TextureId,
                AnchorX = dto.AnchorX,
                AnchorY = dto.AnchorY,
                AnchorZ = dto.AnchorZ,
                Scale = dto.Scale,
                RotationX = dto.RotationX,
                RotationY = dto.RotationY,
                RotationZ = dto.RotationZ
            });
            count++;
        }

        Console.WriteLine($"[Model3DSeeder] 3D models: {count}");
    }
}