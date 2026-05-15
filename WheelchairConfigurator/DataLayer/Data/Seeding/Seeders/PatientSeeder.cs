using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Seeding.Seeders;

public class PatientSeeder
{
    public void Seed(SQLiteConnection db, List<PatientDto> dtos, Dictionary<string, int> specialistMap)
    {
        int count = 0;
        foreach (var dto in dtos)
        {
            if (!specialistMap.TryGetValue(dto.CreatedBySpecialistFullName, out int specialistId))
            {
                Console.WriteLine($"[PatientSeeder] WARN — specialist '{dto.CreatedBySpecialistFullName}' not found for patient '{dto.BirthNumber}'. Skipping.");
                continue;
            }

            var entity = new Patient
            {
                BirthNumber = dto.BirthNumber.Trim(),
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBySpecialistId = specialistId,
                CreatedBySpecialistName = dto.CreatedBySpecialistFullName
            };
            db.Insert(entity);
            count++;
        }

        Console.WriteLine($"[PatientSeeder] Patients: {count}");
    }
}
