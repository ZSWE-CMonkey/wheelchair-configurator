using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Data.Seeding.Seeders;
using WheelchairConfigurator.Domain.Models;
using Xunit;

namespace WheelchairConfigurator.DataLayer.Tests.Seeding;

/// <summary>
/// Integration tests for ComponentSeeder using an in-memory SQLite database.
/// Each test gets its own isolated database — no shared state between tests.
/// </summary>
public class ComponentSeederTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static SQLiteConnection CreateDb()
    {
        var db = new SQLiteConnection(":memory:");
        db.CreateTable<Category>();
        db.CreateTable<Component>();
        return db;
    }

    private static Dictionary<string, int> MakeCategoryMap(params string[] names)
    {
        var map = new Dictionary<string, int>();
        for (int i = 0; i < names.Length; i++)
            map[names[i]] = i + 1;
        return map;
    }

    private static ComponentDto MakeDto(
        string name = "SportWheel X1",
        string categoryName = "Wheels",
        decimal price = 299.99m,
        string? catalogUrl = "https://example.com") => new()
    {
        Name = name,
        CategoryName = categoryName,
        Price = price,
        CatalogUrl = catalogUrl
    };

    // -------------------------------------------------------------------------
    // Insert behavior
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_SingleComponent_InsertsOneRow()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Wheels");

        seeder.Seed(db, [MakeDto()], categoryMap);

        Assert.Equal(1, db.Table<Component>().Count());
    }

    [Fact]
    public void Seed_MultipleComponents_InsertsAllRows()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Wheels", "Frames");

        seeder.Seed(db, [
            MakeDto("Wheel A", "Wheels"),
            MakeDto("Wheel B", "Wheels"),
            MakeDto("Frame A", "Frames")
        ], categoryMap);

        Assert.Equal(3, db.Table<Component>().Count());
    }

    [Fact]
    public void Seed_EmptyList_InsertsNothing()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();

        seeder.Seed(db, [], MakeCategoryMap());

        Assert.Equal(0, db.Table<Component>().Count());
    }

    // -------------------------------------------------------------------------
    // Field mapping
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_Component_NameIsPersisted()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Wheels");

        seeder.Seed(db, [MakeDto("SportWheel X1", "Wheels")], categoryMap);

        Assert.Equal("SportWheel X1", db.Table<Component>().First().Name);
    }

    [Fact]
    public void Seed_Component_PriceIsPersisted()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Wheels");

        seeder.Seed(db, [MakeDto(price: 499.99m)], categoryMap);

        Assert.Equal(499.99m, db.Table<Component>().First().Price);
    }

    [Fact]
    public void Seed_Component_CatalogUrlIsPersisted()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Wheels");

        seeder.Seed(db, [MakeDto(catalogUrl: "https://catalog.example.com/item/1")], categoryMap);

        Assert.Equal("https://catalog.example.com/item/1", db.Table<Component>().First().CatalogUrl);
    }

    [Fact]
    public void Seed_Component_NullCatalogUrl_IsPersisted()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Wheels");

        seeder.Seed(db, [MakeDto(catalogUrl: null)], categoryMap);

        Assert.Null(db.Table<Component>().First().CatalogUrl);
    }

    [Fact]
    public void Seed_Component_CategoryIdIsResolvedFromMap()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = new Dictionary<string, int> { ["Wheels"] = 42 };

        seeder.Seed(db, [MakeDto(categoryName: "Wheels")], categoryMap);

        Assert.Equal(42, db.Table<Component>().First().CategoryId);
    }

    // -------------------------------------------------------------------------
    // Skip behavior — unknown category
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_UnknownCategory_ComponentIsSkipped()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Frames"); // "Wheels" is missing

        seeder.Seed(db, [MakeDto(categoryName: "Wheels")], categoryMap);

        Assert.Equal(0, db.Table<Component>().Count());
    }

    [Fact]
    public void Seed_MixedKnownAndUnknownCategories_OnlyKnownAreInserted()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Wheels"); // "Frames" is missing

        seeder.Seed(db, [
            MakeDto("Wheel A", "Wheels"),
            MakeDto("Frame A", "Frames")  // should be skipped
        ], categoryMap);

        Assert.Equal(1, db.Table<Component>().Count());
        Assert.Equal("Wheel A", db.Table<Component>().First().Name);
    }

    [Fact]
    public void Seed_EmptyCategoryMap_AllComponentsAreSkipped()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();

        seeder.Seed(db, [MakeDto(), MakeDto("Frame A", "Frames")], new Dictionary<string, int>());

        Assert.Equal(0, db.Table<Component>().Count());
    }

    // -------------------------------------------------------------------------
    // Return value — name → ID map
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_ReturnsMapWithCorrectCount()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Wheels", "Frames");

        var map = seeder.Seed(db, [
            MakeDto("Wheel A", "Wheels"),
            MakeDto("Frame A", "Frames")
        ], categoryMap);

        Assert.Equal(2, map.Count);
    }

    [Fact]
    public void Seed_ReturnsMapContainingInsertedNames()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Wheels");

        var map = seeder.Seed(db, [MakeDto("Wheel A", "Wheels")], categoryMap);

        Assert.True(map.ContainsKey("Wheel A"));
    }

    [Fact]
    public void Seed_ReturnedIds_MatchActualDatabaseIds()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Wheels");

        var map = seeder.Seed(db, [MakeDto("Wheel A", "Wheels")], categoryMap);

        var dbId = db.Table<Component>().First(c => c.Name == "Wheel A").Id;
        Assert.Equal(dbId, map["Wheel A"]);
    }

    [Fact]
    public void Seed_SkippedComponents_AreNotInReturnedMap()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();
        var categoryMap = MakeCategoryMap("Wheels");

        var map = seeder.Seed(db, [
            MakeDto("Wheel A", "Wheels"),
            MakeDto("Frame A", "Frames") // skipped
        ], categoryMap);

        Assert.True(map.ContainsKey("Wheel A"));
        Assert.False(map.ContainsKey("Frame A"));
    }

    [Fact]
    public void Seed_EmptyList_ReturnsEmptyMap()
    {
        var db = CreateDb();
        var seeder = new ComponentSeeder();

        var map = seeder.Seed(db, [], MakeCategoryMap("Wheels"));

        Assert.Empty(map);
    }
}
