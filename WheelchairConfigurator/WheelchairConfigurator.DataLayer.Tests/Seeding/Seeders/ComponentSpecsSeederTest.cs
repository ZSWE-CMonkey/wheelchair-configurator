using SQLite;
using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Data.Seeding.Seeders;
using WheelchairConfigurator.Domain.Models;
using Xunit;

namespace WheelchairConfigurator.DataLayer.Tests.Seeding;

/// <summary>
/// Integration tests for ComponentSpecsSeeder using an in-memory SQLite database.
/// </summary>
public class ComponentSpecsSeederTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static SQLiteConnection CreateDb()
    {
        var db = new SQLiteConnection(":memory:");
        db.CreateTable<ComponentSpecs>();
        return db;
    }

    private static Dictionary<string, int> MakeComponentMap(params string[] names)
    {
        var map = new Dictionary<string, int>();
        for (int i = 0; i < names.Length; i++)
            map[names[i]] = i + 1;
        return map;
    }

    private static ComponentSpecsDto MakeDto(string componentName = "Wheel A") => new()
    {
        ComponentName = componentName,
        WeightCapacityKg = 120,
        SeatWidthCm = 45,
        SeatDepthCm = 40,
        BackrestHeightLevel = 2,
        MaxSpeedKmh = 10,
        DrivePowerLevel = 1,
        SupportsTilt = true,
        SupportsRecline = false,
        SupportsLateralSupport = true,
        HasHeadSupport = false,
        PressureReliefLevel = 3,
        ControlMode = "Joystick",
        EnvironmentType = "Indoor",
        SupportsLegRestAdjustment = true,
        ComfortLevel = 2
    };

    // -------------------------------------------------------------------------
    // Insert behavior
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_SingleSpec_InsertsOneRow()
    {
        var db = CreateDb();
        var seeder = new ComponentSpecsSeeder();
        var componentMap = MakeComponentMap("Wheel A");

        seeder.Seed(db, [MakeDto("Wheel A")], componentMap);

        Assert.Equal(1, db.Table<ComponentSpecs>().Count());
    }

    [Fact]
    public void Seed_MultipleSpecs_InsertsAllRows()
    {
        var db = CreateDb();
        var seeder = new ComponentSpecsSeeder();
        var componentMap = MakeComponentMap("Wheel A", "Frame B", "Joystick C");

        seeder.Seed(db, [MakeDto("Wheel A"), MakeDto("Frame B"), MakeDto("Joystick C")], componentMap);

        Assert.Equal(3, db.Table<ComponentSpecs>().Count());
    }

    [Fact]
    public void Seed_EmptyList_InsertsNothing()
    {
        var db = CreateDb();
        var seeder = new ComponentSpecsSeeder();

        seeder.Seed(db, [], MakeComponentMap());

        Assert.Equal(0, db.Table<ComponentSpecs>().Count());
    }

    // -------------------------------------------------------------------------
    // Field mapping
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_ComponentId_IsResolvedFromMap()
    {
        var db = CreateDb();
        var seeder = new ComponentSpecsSeeder();
        var componentMap = new Dictionary<string, int> { ["Wheel A"] = 99 };

        seeder.Seed(db, [MakeDto("Wheel A")], componentMap);

        Assert.Equal(99, db.Table<ComponentSpecs>().First().ComponentId);
    }

    [Fact]
    public void Seed_AllSpecFields_ArePersisted()
    {
        var db = CreateDb();
        var seeder = new ComponentSpecsSeeder();
        var componentMap = MakeComponentMap("Wheel A");

        var dto = MakeDto("Wheel A");
        seeder.Seed(db, [dto], componentMap);

        var inserted = db.Table<ComponentSpecs>().First();
        Assert.Equal(dto.WeightCapacityKg, inserted.WeightCapacityKg);
        Assert.Equal(dto.SeatWidthCm, inserted.SeatWidthCm);
        Assert.Equal(dto.SeatDepthCm, inserted.SeatDepthCm);
        Assert.Equal(dto.BackrestHeightLevel, inserted.BackrestHeightLevel);
        Assert.Equal(dto.MaxSpeedKmh, inserted.MaxSpeedKmh);
        Assert.Equal(dto.DrivePowerLevel, inserted.DrivePowerLevel);
        Assert.Equal(dto.SupportsTilt, inserted.SupportsTilt);
        Assert.Equal(dto.SupportsRecline, inserted.SupportsRecline);
        Assert.Equal(dto.SupportsLateralSupport, inserted.SupportsLateralSupport);
        Assert.Equal(dto.HasHeadSupport, inserted.HasHeadSupport);
        Assert.Equal(dto.PressureReliefLevel, inserted.PressureReliefLevel);
        Assert.Equal(dto.ControlMode, inserted.ControlMode);
        Assert.Equal(dto.EnvironmentType, inserted.EnvironmentType);
        Assert.Equal(dto.SupportsLegRestAdjustment, inserted.SupportsLegRestAdjustment);
        Assert.Equal(dto.ComfortLevel, inserted.ComfortLevel);
    }

    // -------------------------------------------------------------------------
    // Skip behavior — unknown component
    // -------------------------------------------------------------------------

    [Fact]
    public void Seed_UnknownComponent_SpecIsSkipped()
    {
        var db = CreateDb();
        var seeder = new ComponentSpecsSeeder();
        var componentMap = MakeComponentMap("Frame B"); // "Wheel A" is missing

        seeder.Seed(db, [MakeDto("Wheel A")], componentMap);

        Assert.Equal(0, db.Table<ComponentSpecs>().Count());
    }

    [Fact]
    public void Seed_MixedKnownAndUnknown_OnlyKnownAreInserted()
    {
        var db = CreateDb();
        var seeder = new ComponentSpecsSeeder();
        var componentMap = MakeComponentMap("Wheel A"); // "Frame B" is missing

        seeder.Seed(db, [MakeDto("Wheel A"), MakeDto("Frame B")], componentMap);

        Assert.Equal(1, db.Table<ComponentSpecs>().Count());
    }
}
