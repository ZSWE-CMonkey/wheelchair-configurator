using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.Service;
using WheelchairConfigurator.Data.Seeding;

namespace WheelchairConfigurator.Data;

/// <summary>
/// Responsible for database initialization and seeding of initial data.
/// Call <see cref="Initialize"/> once at application startup.
/// </summary>
public class DbInitializer
{
    private readonly DbService _dbService;
    private readonly DataService _dataService;
    private readonly DbSeeder _seeder = new();

    public DbInitializer(DbService dbService, DataService dataService)
    {
        _dbService = dbService;
        _dataService = dataService;
    }

    /// <summary>
    /// Initializes the database and seeds initial data if empty.
    /// </summary>
    /// <param name="resetOnStart">
    /// If true, drops and recreates all tables before seeding.
    /// Use during development only — will erase all existing data.
    /// </param>
    public void Initialize(bool resetOnStart = false)
    {
        var db = _dbService.GetConnection();

        if (resetOnStart)
        {
            Console.WriteLine("[DbInitializer] Reset flag set. Clearing existing data...");
            _dbService.ResetDatabase();
        }

        if (db.Table<Category>().Count() > 0)
        {
            Console.WriteLine("[DbInitializer] Database already contains data. Skipping seed.");
            return;
        }

        Console.WriteLine("[DbInitializer] Empty database detected. Starting seed...");

        foreach (var seedData in _dataService.ProcessData())
        {
            _seeder.Seed(db, seedData);
        }

        Console.WriteLine("[DbInitializer] Initialization complete.");
    }
}