using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Mappers;
using WheelchairConfigurator.ServiceLayer.Models;
using Xunit;

namespace WheelchairConfigurator.ServiceLayer.Tests.Mappers;

public class CategoryMapperTest
{
    // -------------------------------------------------------------------------
    // Map: Category → CategoryModel
    // -------------------------------------------------------------------------

    [Fact]
    public void Map_MapsIdCorrectly()
    {
        var entity = new Category { Id = 42, Name = "Wheels" };
        var result = CategoryMapper.Map(entity);
        Assert.Equal(42, result.Id);
    }

    [Fact]
    public void Map_MapsNameCorrectly()
    {
        var entity = new Category { Id = 1, Name = "Frames" };
        var result = CategoryMapper.Map(entity);
        Assert.Equal("Frames", result.Name);
    }

    [Fact]
    public void Map_ReturnsCategoryModel()
    {
        var entity = new Category { Id = 1, Name = "Test" };
        var result = CategoryMapper.Map(entity);
        Assert.IsType<CategoryModel>(result);
    }

    [Fact]
    public void Map_EmptyName_MapsCorrectly()
    {
        var entity = new Category { Id = 5, Name = string.Empty };
        var result = CategoryMapper.Map(entity);
        Assert.Equal(string.Empty, result.Name);
    }

    [Fact]
    public void Map_ZeroId_MapsCorrectly()
    {
        var entity = new Category { Id = 0, Name = "Footrests" };
        var result = CategoryMapper.Map(entity);
        Assert.Equal(0, result.Id);
    }

    [Theory]
    [InlineData(1, "Wheels")]
    [InlineData(99, "Armrests")]
    [InlineData(int.MaxValue, "Special Category")]
    public void Map_VariousEntities_MapsIdAndNameCorrectly(int id, string name)
    {
        var entity = new Category { Id = id, Name = name };
        var result = CategoryMapper.Map(entity);

        Assert.Equal(id, result.Id);
        Assert.Equal(name, result.Name);
    }

    [Fact]
    public void Map_TwoDistinctEntities_ReturnTwoDistinctModels()
    {
        var e1 = new Category { Id = 1, Name = "A" };
        var e2 = new Category { Id = 2, Name = "B" };

        var r1 = CategoryMapper.Map(e1);
        var r2 = CategoryMapper.Map(e2);

        Assert.NotEqual(r1.Id, r2.Id);
        Assert.NotEqual(r1.Name, r2.Name);
    }
}
