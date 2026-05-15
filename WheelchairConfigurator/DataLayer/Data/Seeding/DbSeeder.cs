using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Data.Seeding.Seeders;

namespace WheelchairConfigurator.Data.Seeding;

/// <summary>
/// Orchestrates the seeding process in the correct dependency order.
/// Runs everything within a single transaction — all or nothing.
/// </summary>
public class DbSeeder
{
    private readonly CategorySeeder _categorySeeder = new();
    private readonly ComponentSeeder _componentSeeder = new();
    private readonly ComponentSpecsSeeder _specsSeeder = new();
    private readonly Model3DSeeder _model3DSeeder = new();
    private readonly CompatibilityRuleSeeder _compatibilityRuleSeeder = new();
    private readonly SpecialistSeeder _specialistSeeder = new();
    private readonly PatientSeeder _patientSeeder = new();
    private readonly PatientMeasurementSeeder _measurementSeeder = new();

    /// <summary>
    /// Seeds all entities from the provided <see cref="SeedDataDto"/> in dependency order.
    /// Rolls back the entire operation if any error occurs.
    /// </summary>
    public void Seed(SQLiteConnection db, SeedDataDto data)
    {
        Console.WriteLine("[DbSeeder] Starting seed...");

        db.BeginTransaction();
        try
        {
            var categoryMap = _categorySeeder.Seed(db, data.Categories);
            var componentMap = _componentSeeder.Seed(db, data.Components, categoryMap);
            _specsSeeder.Seed(db, data.Specs, componentMap);
            _model3DSeeder.Seed(db, data.Models3D, componentMap);
            _compatibilityRuleSeeder.Seed(db, data.Rules, componentMap);
            var specialistMap = _specialistSeeder.Seed(db, data.Specialists);
            var patientMap = _patientSeeder.Seed(db, data.Patients, specialistMap);
            _measurementSeeder.Seed(db, data.Measurements, patientMap, specialistMap);

            db.Commit();
            Console.WriteLine("[DbSeeder] Seed completed successfully.");
        }
        catch (Exception ex)
        {
            db.Rollback();
            Console.WriteLine($"[DbSeeder] ERROR — rollback executed. Detail: {ex.Message}");
            throw;
        }
    }
}