using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Seeding.Seeders;

public class PatientMeasurementSeeder
{
    public void Seed(SQLiteConnection db, List<PatientMeasurementDto> dtos,
        Dictionary<string, int> patientMap, Dictionary<string, int> specialistMap)
    {
        int count = 0;
        foreach (var dto in dtos)
        {
            if (!patientMap.TryGetValue(dto.PatientBirthNumber, out int patientId))
            {
                Console.WriteLine($"[PatientMeasurementSeeder] WARN — patient '{dto.PatientBirthNumber}' not found. Skipping.");
                continue;
            }
            if (!specialistMap.TryGetValue(dto.CreatedBySpecialistFullName, out int specialistId))
            {
                Console.WriteLine($"[PatientMeasurementSeeder] WARN — specialist '{dto.CreatedBySpecialistFullName}' not found. Skipping.");
                continue;
            }

            var entity = new PatientMeasurement
            {
                PatientId = patientId,
                MeasuredAt = DateTime.Now,
                CreatedBySpecialistId = specialistId,
                CreatedBySpecialistName = dto.CreatedBySpecialistFullName,
                BodyHeight = dto.BodyHeight,
                PelvisWidth = dto.PelvisWidth,
                ThighLength = dto.ThighLength,
                Weight = dto.Weight,
                BodyStability = dto.BodyStability,
                HeadStability = dto.HeadStability,
                BedsoreRisk = dto.BedsoreRisk,
                Control = dto.Control,
                Environment = dto.Environment,
                Legs = dto.Legs,
                Pain = dto.Pain
            };
            db.Insert(entity);
            count++;
        }

        Console.WriteLine($"[PatientMeasurementSeeder] Measurements: {count}");
    }
}
