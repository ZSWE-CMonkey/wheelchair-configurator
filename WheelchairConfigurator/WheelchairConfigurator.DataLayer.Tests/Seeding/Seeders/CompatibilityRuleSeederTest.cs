using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Data.Seeding.Seeders;
using WheelchairConfigurator.Domain.Models;
using Xunit;

namespace WheelchairConfigurator.DataLayer.Tests.Seeding;

/// <summary>
/// Integration tests for CompatibilityRuleSeeder using an in-memory SQLite database.
/// </summary>
public class CompatibilityRuleSeederTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static SQLiteConnection CreateDb()
    {
        var db = new SQLiteConnection(":memory:");
        db.CreateTable<CompatibilityRule>();
        return db;
    }

    private static Dictionary<string, int> MakeComponentMap(params string[] names)
    {
        var map = new Dictionary<string, int>();
        for (int i = 0; i < names.Length; i++)
            map[names[i]] = i + 1;
        return map;
    }

    private static CompatibilityRuleDto MakeDto(
        string compA = "Wheel A",
        string compB = "Frame B",
        bool isCompatible = true) => new()
    {
        ComponentAName = compA,
        ComponentBName = compB,
        IsCompatible = isCompatible
    };

    // -------------------------------------------------------------------------
    // Insert behavior
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_SingleRule_InsertsOneRow()
    {
        var db = CreateDb();
        var seeder = new CompatibilityRuleSeeder();
        var componentMap = MakeComponentMap("Wheel A", "Frame B");

        seeder.Seed(db, [MakeDto()], componentMap);

        Assert.Equal(1, db.Table<CompatibilityRule>().Count());
    }

    [Fact]
    public void Seed_MultipleRules_InsertsAllRows()
    {
        var db = CreateDb();
        var seeder = new CompatibilityRuleSeeder();
        var componentMap = MakeComponentMap("Wheel A", "Frame B", "Joystick C");

        seeder.Seed(db, [
            MakeDto("Wheel A", "Frame B", true),
            MakeDto("Wheel A", "Joystick C", false)
        ], componentMap);

        Assert.Equal(2, db.Table<CompatibilityRule>().Count());
    }

    [Fact]
    public void Seed_EmptyList_InsertsNothing()
    {
        var db = CreateDb();
        var seeder = new CompatibilityRuleSeeder();

        seeder.Seed(db, [], MakeComponentMap());

        Assert.Equal(0, db.Table<CompatibilityRule>().Count());
    }

    // -------------------------------------------------------------------------
    // Field mapping
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_ComponentAId_IsResolvedFromMap()
    {
        var db = CreateDb();
        var seeder = new CompatibilityRuleSeeder();
        var componentMap = new Dictionary<string, int> { ["Wheel A"] = 10, ["Frame B"] = 20 };

        seeder.Seed(db, [MakeDto("Wheel A", "Frame B")], componentMap);

        Assert.Equal(10, db.Table<CompatibilityRule>().First().ComponentAId);
    }

    [Fact]
    public void Seed_ComponentBId_IsResolvedFromMap()
    {
        var db = CreateDb();
        var seeder = new CompatibilityRuleSeeder();
        var componentMap = new Dictionary<string, int> { ["Wheel A"] = 10, ["Frame B"] = 20 };

        seeder.Seed(db, [MakeDto("Wheel A", "Frame B")], componentMap);

        Assert.Equal(20, db.Table<CompatibilityRule>().First().ComponentBId);
    }

    [Fact]
    public void Seed_IsCompatibleTrue_IsPersistedCorrectly()
    {
        var db = CreateDb();
        var seeder = new CompatibilityRuleSeeder();
        var componentMap = MakeComponentMap("Wheel A", "Frame B");

        seeder.Seed(db, [MakeDto(isCompatible: true)], componentMap);

        Assert.True(db.Table<CompatibilityRule>().First().IsCompatible);
    }

    [Fact]
    public void Seed_IsCompatibleFalse_IsPersistedCorrectly()
    {
        var db = CreateDb();
        var seeder = new CompatibilityRuleSeeder();
        var componentMap = MakeComponentMap("Wheel A", "Frame B");

        seeder.Seed(db, [MakeDto(isCompatible: false)], componentMap);

        Assert.False(db.Table<CompatibilityRule>().First().IsCompatible);
    }

    // -------------------------------------------------------------------------
    // Skip behavior — unknown components
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_ComponentANotFound_RuleIsSkipped()
    {
        var db = CreateDb();
        var seeder = new CompatibilityRuleSeeder();
        var componentMap = MakeComponentMap("Frame B"); // "Wheel A" is missing

        seeder.Seed(db, [MakeDto("Wheel A", "Frame B")], componentMap);

        Assert.Equal(0, db.Table<CompatibilityRule>().Count());
    }

    [Fact]
    public void Seed_ComponentBNotFound_RuleIsSkipped()
    {
        var db = CreateDb();
        var seeder = new CompatibilityRuleSeeder();
        var componentMap = MakeComponentMap("Wheel A"); // "Frame B" is missing

        seeder.Seed(db, [MakeDto("Wheel A", "Frame B")], componentMap);

        Assert.Equal(0, db.Table<CompatibilityRule>().Count());
    }

    [Fact]
    public void Seed_BothComponentsNotFound_RuleIsSkipped()
    {
        var db = CreateDb();
        var seeder = new CompatibilityRuleSeeder();

        seeder.Seed(db, [MakeDto()], new Dictionary<string, int>());

        Assert.Equal(0, db.Table<CompatibilityRule>().Count());
    }

    [Fact]
    public void Seed_MixedValidAndInvalidRules_OnlyValidAreInserted()
    {
        var db = CreateDb();
        var seeder = new CompatibilityRuleSeeder();
        var componentMap = MakeComponentMap("Wheel A", "Frame B"); // "Joystick C" missing

        seeder.Seed(db, [
            MakeDto("Wheel A", "Frame B"),       // valid
            MakeDto("Wheel A", "Joystick C")     // skipped
        ], componentMap);

        Assert.Equal(1, db.Table<CompatibilityRule>().Count());
    }
}
