using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Seeding.Seeders;

public class SpecialistSeeder
{
    public Dictionary<string, int> Seed(SQLiteConnection db, List<SpecialistDto> dtos)
    {
        var map = new Dictionary<string, int>();

        foreach (var dto in dtos)
        {
            var entity = new Specialist
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email,
                Clinic = dto.Clinic,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            db.Insert(entity);
            var fullName = $"{entity.FirstName} {entity.LastName}";
            map[fullName] = entity.Id;
        }

        Console.WriteLine($"[SpecialistSeeder] Specialists: {map.Count}");
        return map;
    }
}
