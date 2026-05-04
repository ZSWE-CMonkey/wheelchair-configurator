using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Data.Seeding;
using WheelchairConfigurator.Domain.Models;
using Xunit;

namespace WheelchairConfigurator.DataLayer.Tests.Seeding;

/// <summary>
/// Integration tests for DbSeeder using an in-memory SQLite database.
/// Verifies correct seeding order, transaction commit, and rollback on failure.
/// </summary>
public class DbSeederTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static SQLiteConnection CreateDb()
    {
        var db = new SQLiteConnection(":memory:");
        db.CreateTable<Category>();
        db.CreateTable<Component>();
        db.CreateTable<ComponentSpecs>();
        db.CreateTable<Model3D>();
        db.CreateTable<CompatibilityRule>();
        return db;
    }

    private static SeedDataDto MakeFullSeedData() => new()
    {
        Categories =
        [
            new CategoryDto { Name = "Wheels", RoleKey = "wheels" },
            new CategoryDto { Name = "Frames", RoleKey = "frames" }
        ],
        Components =
        [
            new ComponentDto { Name = "Wheel A", CategoryName = "Wheels", Price = 299m },
            new ComponentDto { Name = "Frame B", CategoryName = "Frames", Price = 499m }
        ],
        Specs =
        [
            new ComponentSpecsDto { ComponentName = "Wheel A", WeightCapacityKg = 120, SeatWidthCm = 45 }
        ],
        Models3D =
        [
            new Model3DDto { ComponentName = "Wheel A", FilePath = "models/wheel.obj", TextureId = "tex_001", AnchorX = 0.0m, AnchorY = 0.0m, AnchorZ = 0.0m }
        ],
        Rules =
        [
            new CompatibilityRuleDto { ComponentAName = "Wheel A", ComponentBName = "Frame B", IsCompatible = true }
        ]
    };

    // -------------------------------------------------------------------------
    // Full seed pipeline
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_FullData_InsertsCategories()
    {
        var db = CreateDb();
        var seeder = new DbSeeder();

        seeder.Seed(db, MakeFullSeedData());

        Assert.Equal(2, db.Table<Category>().Count());
    }

    [Fact]
    public void Seed_FullData_InsertsComponents()
    {
        var db = CreateDb();
        var seeder = new DbSeeder();

        seeder.Seed(db, MakeFullSeedData());

        Assert.Equal(2, db.Table<Component>().Count());
    }

    [Fact]
    public void Seed_FullData_InsertsSpecs()
    {
        var db = CreateDb();
        var seeder = new DbSeeder();

        seeder.Seed(db, MakeFullSeedData());

        Assert.Equal(1, db.Table<ComponentSpecs>().Count());
    }

    [Fact]
    public void Seed_FullData_InsertsModels3D()
    {
        var db = CreateDb();
        var seeder = new DbSeeder();

        seeder.Seed(db, MakeFullSeedData());

        Assert.Equal(1, db.Table<Model3D>().Count());
    }

    [Fact]
    public void Seed_FullData_InsertsCompatibilityRules()
    {
        var db = CreateDb();
        var seeder = new DbSeeder();

        seeder.Seed(db, MakeFullSeedData());

        Assert.Equal(1, db.Table<CompatibilityRule>().Count());
    }

    [Fact]
    public void Seed_EmptyData_InsertsNothing()
    {
        var db = CreateDb();
        var seeder = new DbSeeder();

        seeder.Seed(db, new SeedDataDto());

        Assert.Equal(0, db.Table<Category>().Count());
        Assert.Equal(0, db.Table<Component>().Count());
        Assert.Equal(0, db.Table<ComponentSpecs>().Count());
        Assert.Equal(0, db.Table<Model3D>().Count());
        Assert.Equal(0, db.Table<CompatibilityRule>().Count());
    }

    // -------------------------------------------------------------------------
    // Dependency order — components resolve category IDs correctly
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_Components_ResolveCategoryIdsFromInsertedCategories()
    {
        var db = CreateDb();
        var seeder = new DbSeeder();

        seeder.Seed(db, MakeFullSeedData());

        var wheelsCategoryId = db.Table<Category>().First(c => c.Name == "Wheels").Id;
        var wheelA = db.Table<Component>().First(c => c.Name == "Wheel A");
        Assert.Equal(wheelsCategoryId, wheelA.CategoryId);
    }

    [Fact]
    public void Seed_Specs_ResolveComponentIdsFromInsertedComponents()
    {
        var db = CreateDb();
        var seeder = new DbSeeder();

        seeder.Seed(db, MakeFullSeedData());

        var wheelAId = db.Table<Component>().First(c => c.Name == "Wheel A").Id;
        var specs = db.Table<ComponentSpecs>().First();
        Assert.Equal(wheelAId, specs.ComponentId);
    }

    // -------------------------------------------------------------------------
    // Transaction — rollback on failure
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_WhenExceptionOccurs_RollsBackAllChanges()
    {
        var db = CreateDb();
        var seeder = new DbSeeder();

        // Drop CompatibilityRule table to force an exception mid-seed
        db.DropTable<CompatibilityRule>();

        var data = MakeFullSeedData();
        Assert.Throws<SQLiteException>(() => seeder.Seed(db, data));

        // Transaction rollback — categories and components inserted before crash must be undone
        Assert.Equal(0, db.Table<Category>().Count());
        Assert.Equal(0, db.Table<Component>().Count());
    }

    [Fact]
    public void Seed_WhenExceptionOccurs_RethrowsException()
    {
        var db = CreateDb();
        var seeder = new DbSeeder();

        db.DropTable<CompatibilityRule>();

        Assert.Throws<SQLiteException>(() => seeder.Seed(db, MakeFullSeedData()));
    }
}