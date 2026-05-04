using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Data.Seeding.Seeders;
using WheelchairConfigurator.Domain.Models;
using Xunit;

namespace WheelchairConfigurator.DataLayer.Tests.Seeding;

/// <summary>
/// Integration tests for Model3DSeeder using an in-memory SQLite database.
/// </summary>
public class Model3DSeederTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static SQLiteConnection CreateDb()
    {
        var db = new SQLiteConnection(":memory:");
        db.CreateTable<Model3D>();
        return db;
    }

    private static Dictionary<string, int> MakeComponentMap(params string[] names)
    {
        var map = new Dictionary<string, int>();
        for (int i = 0; i < names.Length; i++)
            map[names[i]] = i + 1;
        return map;
    }

    private static Model3DDto MakeDto(string componentName = "Wheel A") => new()
    {
        ComponentName = componentName,
        FilePath = "models/wheel_a.obj",
        TextureId = "tex_001",
        AnchorX = 1.5m,
        AnchorY = 0.0m,
        AnchorZ = -0.5m
    };

    // -------------------------------------------------------------------------
    // Insert behavior
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_SingleModel_InsertsOneRow()
    {
        var db = CreateDb();
        var seeder = new Model3DSeeder();
        var componentMap = MakeComponentMap("Wheel A");

        seeder.Seed(db, [MakeDto("Wheel A")], componentMap);

        Assert.Equal(1, db.Table<Model3D>().Count());
    }

    [Fact]
    public void Seed_MultipleModels_InsertsAllRows()
    {
        var db = CreateDb();
        var seeder = new Model3DSeeder();
        var componentMap = MakeComponentMap("Wheel A", "Frame B");

        seeder.Seed(db, [MakeDto("Wheel A"), MakeDto("Frame B")], componentMap);

        Assert.Equal(2, db.Table<Model3D>().Count());
    }

    [Fact]
    public void Seed_EmptyList_InsertsNothing()
    {
        var db = CreateDb();
        var seeder = new Model3DSeeder();

        seeder.Seed(db, [], MakeComponentMap());

        Assert.Equal(0, db.Table<Model3D>().Count());
    }

    // -------------------------------------------------------------------------
    // Field mapping
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_ComponentId_IsResolvedFromMap()
    {
        var db = CreateDb();
        var seeder = new Model3DSeeder();
        var componentMap = new Dictionary<string, int> { ["Wheel A"] = 77 };

        seeder.Seed(db, [MakeDto("Wheel A")], componentMap);

        Assert.Equal(77, db.Table<Model3D>().First().ComponentId);
    }

    [Fact]
    public void Seed_AllModelFields_ArePersisted()
    {
        var db = CreateDb();
        var seeder = new Model3DSeeder();
        var componentMap = MakeComponentMap("Wheel A");

        var dto = MakeDto("Wheel A");
        seeder.Seed(db, [dto], componentMap);

        var inserted = db.Table<Model3D>().First();
        Assert.Equal(dto.FilePath, inserted.FilePath);
        Assert.Equal(dto.TextureId, inserted.TextureId);
        Assert.Equal(dto.AnchorX, inserted.AnchorX);
        Assert.Equal(dto.AnchorY, inserted.AnchorY);
        Assert.Equal(dto.AnchorZ, inserted.AnchorZ);
    }

    // -------------------------------------------------------------------------
    // Skip behavior — unknown component
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_UnknownComponent_ModelIsSkipped()
    {
        var db = CreateDb();
        var seeder = new Model3DSeeder();
        var componentMap = MakeComponentMap("Frame B"); // "Wheel A" is missing

        seeder.Seed(db, [MakeDto("Wheel A")], componentMap);

        Assert.Equal(0, db.Table<Model3D>().Count());
    }

    [Fact]
    public void Seed_MixedKnownAndUnknown_OnlyKnownAreInserted()
    {
        var db = CreateDb();
        var seeder = new Model3DSeeder();
        var componentMap = MakeComponentMap("Wheel A"); // "Frame B" is missing

        seeder.Seed(db, [MakeDto("Wheel A"), MakeDto("Frame B")], componentMap);

        Assert.Equal(1, db.Table<Model3D>().Count());
    }
}
