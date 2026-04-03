using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data;

/// <summary>
/// Handles the SQLite database connection and schema initialization.
/// </summary>
public class DbService
{
    private readonly SQLiteConnection _db;
    private readonly SQLiteAsyncConnection _asyncDb;

    public DbService(string dbPath)
    {
        _db = new SQLiteConnection(dbPath);
        _asyncDb = new SQLiteAsyncConnection(dbPath);
        InitializeDatabase();
    }

    /// <summary>
    /// Creates all necessary tables based on the domain models.
    /// </summary>
    private void InitializeDatabase()
    {
        _db.CreateTable<Category>();
        _db.CreateTable<Component>();
        _db.CreateTable<ComponentSpecs>();
        _db.CreateTable<Model3D>();
        _db.CreateTable<CompatibilityRule>();
        _db.CreateTable<Specialist>();
        _db.CreateTable<Configuration>();
        _db.CreateTable<ConfigurationItem>();
    }

    /// <summary>
    /// Drops and recreates all tables. Use during development only.
    /// </summary>
    public void ResetDatabase()
    {
        _db.DropTable<Category>();
        _db.DropTable<Component>();
        _db.DropTable<ComponentSpecs>();
        _db.DropTable<Model3D>();
        _db.DropTable<CompatibilityRule>();
        _db.DropTable<Specialist>();
        _db.DropTable<Configuration>();
        _db.DropTable<ConfigurationItem>();
        InitializeDatabase();
    }

    /// <summary>
    /// Returns the active database connection to be used by repositories.
    /// </summary>
    public SQLiteConnection GetConnection() => _db;
    /// <summary>
    /// Returns the async database connection to be used by repositories.
    /// </summary>
    public SQLiteAsyncConnection GetAsyncConnection() => _asyncDb;
}