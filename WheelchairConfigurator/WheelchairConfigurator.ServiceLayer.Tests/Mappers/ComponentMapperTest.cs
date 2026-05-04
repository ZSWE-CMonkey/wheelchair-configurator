using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Mappers;
using WheelchairConfigurator.ServiceLayer.Models;
using Xunit;

namespace WheelchairConfigurator.ServiceLayer.Tests.Mappers;

public class ComponentMapperTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Component CreateEntity(
        int id = 1,
        string name = "Test Component",
        decimal price = 199.99m,
        string? catalogUrl = "https://example.com/comp") => new()
    {
        Id = id,
        Name = name,
        Price = price,
        CatalogUrl = catalogUrl
    };

    // -------------------------------------------------------------------------
    // Map: Component → ComponentModel
    // -------------------------------------------------------------------------

    [Fact]
    public void Map_MapsIdCorrectly()
    {
        var result = ComponentMapper.Map(CreateEntity(id: 7));
        Assert.Equal(7, result.Id);
    }

    [Fact]
    public void Map_MapsNameCorrectly()
    {
        var result = ComponentMapper.Map(CreateEntity(name: "Aluminium Frame"));
        Assert.Equal("Aluminium Frame", result.Name);
    }

    [Fact]
    public void Map_MapsPriceCorrectly()
    {
        var result = ComponentMapper.Map(CreateEntity(price: 349.50m));
        Assert.Equal(349.50m, result.Price);
    }

    [Fact]
    public void Map_MapsCatalogUrlCorrectly()
    {
        var result = ComponentMapper.Map(CreateEntity(catalogUrl: "https://catalog.example.com/item/1"));
        Assert.Equal("https://catalog.example.com/item/1", result.CatalogUrl);
    }

    [Fact]
    public void Map_NullCatalogUrl_MapsAsNull()
    {
        var result = ComponentMapper.Map(CreateEntity(catalogUrl: null));
        Assert.Null(result.CatalogUrl);
    }

    [Fact]
    public void Map_ReturnsComponentModel()
    {
        var result = ComponentMapper.Map(CreateEntity());
        Assert.IsType<ComponentModel>(result);
    }

    [Fact]
    public void Map_IsRecommended_DefaultsFalse()
    {
        // AppService sets this after engine evaluation — mapper must not pre-set it
        var result = ComponentMapper.Map(CreateEntity());
        Assert.False(result.IsRecommended);
    }

    [Fact]
    public void Map_IsIncompatible_DefaultsFalse()
    {
        // AppService sets this after engine evaluation — mapper must not pre-set it
        var result = ComponentMapper.Map(CreateEntity());
        Assert.False(result.IsIncompatible);
    }

    [Fact]
    public void Map_ZeroPrice_MapsCorrectly()
    {
        var result = ComponentMapper.Map(CreateEntity(price: 0m));
        Assert.Equal(0m, result.Price);
    }

    [Theory]
    [InlineData(1, "Wheel 24\"", 250.00)]
    [InlineData(2, "Carbon Frame", 1200.00)]
    [InlineData(99, "Joystick Controller", 89.99)]
    public void Map_VariousEntities_MapsAllFieldsCorrectly(int id, string name, double price)
    {
        var entity = CreateEntity(id: id, name: name, price: (decimal)price);
        var result = ComponentMapper.Map(entity);

        Assert.Equal(id, result.Id);
        Assert.Equal(name, result.Name);
        Assert.Equal((decimal)price, result.Price);
    }
}
