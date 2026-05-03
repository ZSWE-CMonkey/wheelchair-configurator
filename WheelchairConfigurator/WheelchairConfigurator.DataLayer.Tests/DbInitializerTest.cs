using Moq;
using WheelchairConfigurator.Data;
using WheelchairConfigurator.Data.Providers;
using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.Service;
using Xunit;

namespace WheelchairConfigurator.DataLayer.Tests;

/// <summary>
/// Integration tests for DbInitializer.
/// Uses real DbService with a temp file-based SQLite database and a real DataService
/// backed by a temporary JSON seed file. All temp files are cleaned up after each test.
///
/// These tests verify the conditional seeding logic — the most critical behavior
/// of DbInitializer: seed when empty, skip when has data, reset when asked.
/// </summary>
public class DbInitializerTest : IDisposable
{
    private readonly string _dbPath;
    private readonly string _jsonPath;
    private readonly List<string> _tempFiles = [];

    public DbInitializerTest()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"DbInitializerTest_{Guid.NewGuid()}.db");
        _jsonPath = Path.Combine(Path.GetTempPath(), $"DbInitializerTest_{Guid.NewGuid()}.json");
        _tempFiles.Add(_dbPath);
        _tempFiles.Add(_jsonPath);
    }

    public void Dispose()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        foreach (var file in _tempFiles.Where(File.Exists))
            File.Delete(file);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void WriteMinimalSeedJson(int categoryCount = 1)
    {
        var categories = string.Join(",", Enumerable.Range(1, categoryCount)
            .Select(i => $"{{\"Name\": \"Category{i}\", \"RoleKey\": \"cat{i}\"}}"));

        File.WriteAllText(_jsonPath, $$"""
            {
                "Categories": [{{categories}}],
                "Components": [],
                "Specs": [],
                "Models3D": [],
                "Rules": []
            }
            """);
    }

    private void WriteEmptySeedJson()
    {
        File.WriteAllText(_jsonPath, """
            {
                "Categories": [],
                "Components": [],
                "Specs": [],
                "Models3D": [],
                "Rules": []
            }
            """);
    }

    private (DbService dbService, DbInitializer initializer) CreateSut()
    {
        var dbService = new DbService(_dbPath);
        var mockProvider = new Mock<ILocalFileProvider>();
        mockProvider.Setup(p => p.GetSeedFilePaths()).Returns([_jsonPath]);
        var dataService = new DataService(mockProvider.Object, new JsonDataLoader());
        return (dbService, new DbInitializer(dbService, dataService));
    }

    // -------------------------------------------------------------------------
    // Empty database — seeding runs
    // -------------------------------------------------------------------------

    [Fact]
    public void Initialize_EmptyDatabase_SeedsCategories()
    {
        WriteMinimalSeedJson(categoryCount: 2);
        var (dbService, initializer) = CreateSut();

        initializer.Initialize();

        Assert.Equal(2, dbService.GetConnection().Table<Category>().Count());
    }

    [Fact]
    public void Initialize_EmptyDatabase_WithEmptySeedFile_InsertsNothing()
    {
        WriteEmptySeedJson();
        var (dbService, initializer) = CreateSut();

        initializer.Initialize();

        Assert.Equal(0, dbService.GetConnection().Table<Category>().Count());
    }

    // -------------------------------------------------------------------------
    // Already seeded database — seeding is skipped
    // -------------------------------------------------------------------------

    [Fact]
    public void Initialize_DatabaseAlreadyHasData_DoesNotSeedAgain()
    {
        WriteMinimalSeedJson(categoryCount: 1);
        var (dbService, initializer) = CreateSut();

        initializer.Initialize(); // first run — seeds 1 category
        initializer.Initialize(); // second run — must be skipped

        Assert.Equal(1, dbService.GetConnection().Table<Category>().Count());
    }

    [Fact]
    public void Initialize_DatabaseAlreadyHasData_DoesNotThrow()
    {
        WriteMinimalSeedJson();
        var (dbService, initializer) = CreateSut();

        initializer.Initialize();
        var ex = Record.Exception(() => initializer.Initialize());

        Assert.Null(ex);
    }

    // -------------------------------------------------------------------------
    // resetOnStart = true
    // -------------------------------------------------------------------------

    [Fact]
    public void Initialize_ResetOnStart_ClearsExistingDataAndReseeeds()
    {
        WriteMinimalSeedJson(categoryCount: 1);
        var (dbService, initializer) = CreateSut();

        initializer.Initialize();                       // seeds 1 category
        initializer.Initialize(resetOnStart: true);     // drops, recreates, seeds again

        Assert.Equal(1, dbService.GetConnection().Table<Category>().Count());
    }

    [Fact]
    public void Initialize_ResetOnStart_TablesExistAfterReset()
    {
        WriteEmptySeedJson();
        var (dbService, initializer) = CreateSut();

        initializer.Initialize(resetOnStart: true);

        var db = dbService.GetConnection();
        var tableCount = db.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table'");
        Assert.True(tableCount > 0);
    }

    [Fact]
    public void Initialize_ResetOnStart_WithEmptySeedFile_ResultsInEmptyDatabase()
    {
        WriteMinimalSeedJson(categoryCount: 3);
        var (dbService, initializer) = CreateSut();

        initializer.Initialize();
        Assert.Equal(3, dbService.GetConnection().Table<Category>().Count());

        WriteEmptySeedJson(); // replace seed file with empty data
        initializer.Initialize(resetOnStart: true);

        Assert.Equal(0, dbService.GetConnection().Table<Category>().Count());
    }

    // -------------------------------------------------------------------------
    // Default parameter
    // -------------------------------------------------------------------------

    [Fact]
    public void Initialize_DefaultParameter_DoesNotReset()
    {
        WriteMinimalSeedJson(categoryCount: 2);
        var (dbService, initializer) = CreateSut();

        initializer.Initialize(); // seeds 2 categories
        initializer.Initialize(); // default = no reset, skip because already has data

        Assert.Equal(2, dbService.GetConnection().Table<Category>().Count());
    }
}