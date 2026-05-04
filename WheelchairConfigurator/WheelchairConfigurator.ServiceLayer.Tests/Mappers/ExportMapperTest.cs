using Moq;
using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Mappers;
using Xunit;

namespace WheelchairConfigurator.ServiceLayer.Tests.Mappers;

public class ExportMapperTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Configuration MakeConfig(int id = 1, int specialistId = 10) => new()
    {
        Id = id,
        SpecialistId = specialistId,
        CreatedAt = new DateTime(2024, 5, 1, 12, 0, 0)
    };

    private static Specialist MakeSpecialist(string firstName = "Jana", string lastName = "Nováková") => new()
    {
        Id = 10,
        FirstName = firstName,
        LastName = lastName
    };

    private static Category MakeCategory(int id, string name) =>
        new() { Id = id, Name = name };

    private static Component MakeComponent(int id, string name, int categoryId, decimal price, string? catalogUrl = null) => new()
    {
        Id = id,
        Name = name,
        CategoryId = categoryId,
        Price = price,
        CatalogUrl = catalogUrl
    };

    private static ConfigurationItem MakeItem(int componentId, int qty = 1) => new()
    {
        ComponentId = componentId,
        Quantity = qty
    };

    private static (Mock<IComponentRepository>, Mock<ICategoryRepository>) MakeMocks(
        List<Component> components,
        List<Category> categories)
    {
        var compRepo = new Mock<IComponentRepository>();
        var catRepo = new Mock<ICategoryRepository>();

        compRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(components);
        catRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(categories);

        return (compRepo, catRepo);
    }

    // -------------------------------------------------------------------------
    // ConfigurationName
    // -------------------------------------------------------------------------

    [Fact]
    public async Task MapAsync_ConfigurationName_IncludesConfigId()
    {
        var (compRepo, catRepo) = MakeMocks(
            [MakeComponent(1, "Frame", categoryId: 1, price: 100m)],
            [MakeCategory(1, "Frames")]);

        var result = await ExportMapper.MapAsync(
            MakeConfig(id: 42), [MakeItem(1)], MakeSpecialist(), compRepo.Object, catRepo.Object);

        Assert.Contains("42", result.ConfigurationName);
    }

    // -------------------------------------------------------------------------
    // SpecialistName
    // -------------------------------------------------------------------------

    [Fact]
    public async Task MapAsync_SpecialistName_CombinesFirstAndLastName()
    {
        var (compRepo, catRepo) = MakeMocks(
            [MakeComponent(1, "Wheel", categoryId: 1, price: 200m)],
            [MakeCategory(1, "Wheels")]);

        var result = await ExportMapper.MapAsync(
            MakeConfig(), [MakeItem(1)], MakeSpecialist("Karel", "Novák"), compRepo.Object, catRepo.Object);

        Assert.Equal("Karel Novák", result.SpecialistName);
    }

    [Fact]
    public async Task MapAsync_WhenSpecialistIsNull_UsesUnknownSpecialistFallback()
    {
        var (compRepo, catRepo) = MakeMocks([], []);

        var result = await ExportMapper.MapAsync(
            MakeConfig(), [], null!, compRepo.Object, catRepo.Object);

        Assert.Equal("Unknown Specialist", result.SpecialistName);
    }

    // -------------------------------------------------------------------------
    // CreatedAt
    // -------------------------------------------------------------------------

    [Fact]
    public async Task MapAsync_CreatedAt_MatchesConfigCreatedAt()
    {
        var config = new Configuration
        {
            Id = 1,
            SpecialistId = 10,
            CreatedAt = new DateTime(2023, 11, 15, 8, 30, 0)
        };
        var (compRepo, catRepo) = MakeMocks(
            [MakeComponent(1, "Frame", categoryId: 1, price: 0m)],
            [MakeCategory(1, "Frames")]);

        var result = await ExportMapper.MapAsync(
            config, [MakeItem(1)], MakeSpecialist(), compRepo.Object, catRepo.Object);

        Assert.Equal(config.CreatedAt, result.CreatedAt);
    }

    // -------------------------------------------------------------------------
    // Items — count
    // -------------------------------------------------------------------------

    [Fact]
    public async Task MapAsync_Items_CountMatchesConfigurationItemCount()
    {
        var (compRepo, catRepo) = MakeMocks(
            [MakeComponent(1, "Frame", 1, 100m), MakeComponent(2, "Wheel", 2, 200m)],
            [MakeCategory(1, "Frames"), MakeCategory(2, "Wheels")]);

        var result = await ExportMapper.MapAsync(
            MakeConfig(), [MakeItem(1), MakeItem(2)], MakeSpecialist(), compRepo.Object, catRepo.Object);

        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task MapAsync_EmptyItems_ReturnsEmptyItemsList()
    {
        var (compRepo, catRepo) = MakeMocks([], []);

        var result = await ExportMapper.MapAsync(
            MakeConfig(), [], MakeSpecialist(), compRepo.Object, catRepo.Object);

        Assert.Empty(result.Items);
    }

    // -------------------------------------------------------------------------
    // Items — field mapping
    // -------------------------------------------------------------------------

    [Fact]
    public async Task MapAsync_ExportItem_ComponentNameIsMappedCorrectly()
    {
        var (compRepo, catRepo) = MakeMocks(
            [MakeComponent(1, "Carbon Frame Pro", categoryId: 1, price: 500m)],
            [MakeCategory(1, "Frames")]);

        var result = await ExportMapper.MapAsync(
            MakeConfig(), [MakeItem(1)], MakeSpecialist(), compRepo.Object, catRepo.Object);

        Assert.Equal("Carbon Frame Pro", result.Items[0].ComponentName);
    }

    [Fact]
    public async Task MapAsync_ExportItem_CategoryNameIsMappedCorrectly()
    {
        var (compRepo, catRepo) = MakeMocks(
            [MakeComponent(1, "Frame", categoryId: 7, price: 100m)],
            [MakeCategory(7, "Power Systems")]);

        var result = await ExportMapper.MapAsync(
            MakeConfig(), [MakeItem(1)], MakeSpecialist(), compRepo.Object, catRepo.Object);

        Assert.Equal("Power Systems", result.Items[0].CategoryName);
    }

    [Fact]
    public async Task MapAsync_ExportItem_PriceIsMappedCorrectly()
    {
        var (compRepo, catRepo) = MakeMocks(
            [MakeComponent(1, "Frame", categoryId: 1, price: 349.99m)],
            [MakeCategory(1, "Frames")]);

        var result = await ExportMapper.MapAsync(
            MakeConfig(), [MakeItem(1)], MakeSpecialist(), compRepo.Object, catRepo.Object);

        Assert.Equal(349.99m, result.Items[0].Price);
    }

    [Fact]
    public async Task MapAsync_ExportItem_QuantityIsMappedCorrectly()
    {
        var (compRepo, catRepo) = MakeMocks(
            [MakeComponent(1, "Wheel", categoryId: 1, price: 100m)],
            [MakeCategory(1, "Wheels")]);

        var result = await ExportMapper.MapAsync(
            MakeConfig(), [MakeItem(componentId: 1, qty: 4)], MakeSpecialist(), compRepo.Object, catRepo.Object);

        Assert.Equal(4, result.Items[0].Quantity);
    }

    [Fact]
    public async Task MapAsync_ExportItem_CatalogUrlIsUsedAsItemCode()
    {
        var (compRepo, catRepo) = MakeMocks(
            [MakeComponent(1, "Frame", categoryId: 1, price: 100m, catalogUrl: "CAT-XYZ-001")],
            [MakeCategory(1, "Frames")]);

        var result = await ExportMapper.MapAsync(
            MakeConfig(), [MakeItem(1)], MakeSpecialist(), compRepo.Object, catRepo.Object);

        Assert.Equal("CAT-XYZ-001", result.Items[0].ItemCode);
    }

    [Fact]
    public async Task MapAsync_ExportItem_NullCatalogUrl_ItemCodeIsDash()
    {
        var (compRepo, catRepo) = MakeMocks(
            [MakeComponent(1, "Frame", categoryId: 1, price: 100m, catalogUrl: null)],
            [MakeCategory(1, "Frames")]);

        var result = await ExportMapper.MapAsync(
            MakeConfig(), [MakeItem(1)], MakeSpecialist(), compRepo.Object, catRepo.Object);

        Assert.Equal("-", result.Items[0].ItemCode);
    }

    // -------------------------------------------------------------------------
    // TotalPrice
    // -------------------------------------------------------------------------

    [Fact]
    public async Task MapAsync_TotalPrice_IsSumOfPriceTimesQuantity()
    {
        // Frame x1 = 100, Wheel x3 = 150 → total = 250
        var (compRepo, catRepo) = MakeMocks(
            [MakeComponent(1, "Frame", 1, 100m), MakeComponent(2, "Wheel", 2, 50m)],
            [MakeCategory(1, "Frames"), MakeCategory(2, "Wheels")]);

        var result = await ExportMapper.MapAsync(
            MakeConfig(),
            [MakeItem(componentId: 1, qty: 1), MakeItem(componentId: 2, qty: 3)],
            MakeSpecialist(),
            compRepo.Object,
            catRepo.Object);

        Assert.Equal(250m, result.TotalPrice);
    }

    [Fact]
    public async Task MapAsync_EmptyItems_TotalPriceIsZero()
    {
        var (compRepo, catRepo) = MakeMocks([], []);

        var result = await ExportMapper.MapAsync(
            MakeConfig(), [], MakeSpecialist(), compRepo.Object, catRepo.Object);

        Assert.Equal(0m, result.TotalPrice);
    }

    // -------------------------------------------------------------------------
    // N+1 prevention — bulk load verification
    // -------------------------------------------------------------------------

    [Fact]
    public async Task MapAsync_LoadsAllComponentsInSingleRepositoryCall()
    {
        var compRepo = new Mock<IComponentRepository>();
        var catRepo = new Mock<ICategoryRepository>();

        compRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync([MakeComponent(1, "A", 1, 10m), MakeComponent(2, "B", 1, 20m)]);
        catRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync([MakeCategory(1, "Cat")]);

        await ExportMapper.MapAsync(
            MakeConfig(), [MakeItem(1), MakeItem(2)], MakeSpecialist(), compRepo.Object, catRepo.Object);

        // Must be called exactly once regardless of item count — no N+1
        compRepo.Verify(r => r.GetByIdsAsync(It.IsAny<List<int>>()), Times.Once);
    }

    [Fact]
    public async Task MapAsync_LoadsAllCategoriesInSingleRepositoryCall()
    {
        var compRepo = new Mock<IComponentRepository>();
        var catRepo = new Mock<ICategoryRepository>();

        compRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync([MakeComponent(1, "A", 1, 10m), MakeComponent(2, "B", 2, 20m)]);
        catRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync([MakeCategory(1, "Cat1"), MakeCategory(2, "Cat2")]);

        await ExportMapper.MapAsync(
            MakeConfig(), [MakeItem(1), MakeItem(2)], MakeSpecialist(), compRepo.Object, catRepo.Object);

        catRepo.Verify(r => r.GetByIdsAsync(It.IsAny<List<int>>()), Times.Once);
    }
}
