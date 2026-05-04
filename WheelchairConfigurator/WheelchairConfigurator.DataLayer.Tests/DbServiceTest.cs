using SQLite;
using WheelchairConfigurator.Data;
using WheelchairConfigurator.Domain.Models;
using Xunit;

namespace WheelchairConfigurator.DataLayer.Tests;

/// <summary>
/// Integration tests for DbService using a temporary file-based SQLite database.
/// In-memory SQLite cannot be used here because DbService creates two connections
/// (sync + async) internally — each ":memory:" connection gets its own isolated DB.
/// Temp files are cleaned up after each test via IDisposable.
/// </summary>
public class DbServiceTest : IDisposable
{
    private readonly string _dbPath;
    private readonly DbService _dbService;

    public DbServiceTest()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"DbServiceTest_{Guid.NewGuid()}.db");
        _dbService = new DbService(_dbPath);
    }

    public void Dispose()
    {
        _dbService.Close();
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    // -------------------------------------------------------------------------
    // InitializeDatabase — table creation
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_CreatesAllTables()
    {
        var db = _dbService.GetConnection();

        // All 8 domain tables must exist after construction
        Assert.True(TableExists(db, "Category"));
        Assert.True(TableExists(db, "Component"));
        Assert.True(TableExists(db, "ComponentSpecs"));
        Assert.True(TableExists(db, "Model3D"));
        Assert.True(TableExists(db, "CompatibilityRule"));
        Assert.True(TableExists(db, "Specialist"));
        Assert.True(TableExists(db, "Configuration"));
        Assert.True(TableExists(db, "ConfigurationItem"));
    }

    [Fact]
    public void Constructor_TablesAreWritable()
    {
        var db = _dbService.GetConnection();

        // Insert and verify — proves table is properly initialized, not just created
        db.Insert(new Category { Name = "Wheels", RoleKey = "wheels" });
        Assert.Equal(1, db.Table<Category>().Count());
    }

    // -------------------------------------------------------------------------
    // EnsureSchemaUpgrades — TryAddColumn
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_SchemaUpgrades_RoleKeyColumnExistsOnCategory()
    {
        var db = _dbService.GetConnection();
        var columns = db.GetTableInfo("Category");
        Assert.Contains(columns, c => c.Name == "RoleKey");
    }

    [Fact]
    public void Constructor_SchemaUpgrades_SeatDepthCmColumnExistsOnComponentSpecs()
    {
        var db = _dbService.GetConnection();
        var columns = db.GetTableInfo("ComponentSpecs");
        Assert.Contains(columns, c => c.Name == "SeatDepthCm");
    }

    [Fact]
    public void Constructor_CalledTwiceOnSameDb_DoesNotThrow()
    {
        // Second DbService on the same file — TryAddColumn must be idempotent
        DbService? second = null;
        var ex = Record.Exception(() => second = new DbService(_dbPath));
        second?.Close();
        Assert.Null(ex);
    }

    [Fact]
    public void Constructor_CalledTwiceOnSameDb_DoesNotDuplicateColumns()
    {
        var second = new DbService(_dbPath);
        second.Close();

        var db = _dbService.GetConnection();
        var columns = db.GetTableInfo("Category");
        var roleKeyColumns = columns.Count(c => c.Name == "RoleKey");

        Assert.Equal(1, roleKeyColumns);
    }

    // -------------------------------------------------------------------------
    // ResetDatabase
    // -------------------------------------------------------------------------

    [Fact]
    public void ResetDatabase_ClearsAllExistingData()
    {
        var db = _dbService.GetConnection();
        db.Insert(new Category { Name = "Wheels", RoleKey = "wheels" });
        db.Insert(new Category { Name = "Frames", RoleKey = "frames" });
        Assert.Equal(2, db.Table<Category>().Count());

        _dbService.ResetDatabase();

        Assert.Equal(0, db.Table<Category>().Count());
    }

    [Fact]
    public void ResetDatabase_TablesStillExistAfterReset()
    {
        _dbService.ResetDatabase();

        var db = _dbService.GetConnection();
        Assert.True(TableExists(db, "Category"));
        Assert.True(TableExists(db, "Component"));
        Assert.True(TableExists(db, "ComponentSpecs"));
        Assert.True(TableExists(db, "Model3D"));
        Assert.True(TableExists(db, "CompatibilityRule"));
        Assert.True(TableExists(db, "Specialist"));
        Assert.True(TableExists(db, "Configuration"));
        Assert.True(TableExists(db, "ConfigurationItem"));
    }

    [Fact]
    public void ResetDatabase_TablesAreWritableAfterReset()
    {
        _dbService.ResetDatabase();

        var db = _dbService.GetConnection();
        db.Insert(new Category { Name = "Wheels", RoleKey = "wheels" });
        Assert.Equal(1, db.Table<Category>().Count());
    }

    // -------------------------------------------------------------------------
    // GetConnection / GetAsyncConnection
    // -------------------------------------------------------------------------

    [Fact]
    public void GetConnection_ReturnsNonNull()
    {
        Assert.NotNull(_dbService.GetConnection());
    }

    [Fact]
    public void GetAsyncConnection_ReturnsNonNull()
    {
        Assert.NotNull(_dbService.GetAsyncConnection());
    }

    [Fact]
    public void GetConnection_ReturnsSameInstanceOnEachCall()
    {
        var conn1 = _dbService.GetConnection();
        var conn2 = _dbService.GetConnection();
        Assert.Same(conn1, conn2);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static bool TableExists(SQLiteConnection db, string tableName)
    {
        var count = db.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=?", tableName);
        return count > 0;
    }
}