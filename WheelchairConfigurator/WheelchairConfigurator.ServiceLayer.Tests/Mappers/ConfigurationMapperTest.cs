using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Mappers;
using WheelchairConfigurator.ServiceLayer.Models;
using Xunit;

namespace WheelchairConfigurator.ServiceLayer.Tests.Mappers;

public class ConfigurationMapperTest
{
    // -------------------------------------------------------------------------
    // Map: Configuration (entity) → ConfigurationModel
    // -------------------------------------------------------------------------

    [Fact]
    public void Map_Entity_MapsIdCorrectly()
    {
        var entity = new Configuration { Id = 10, SpecialistId = 2, CreatedAt = DateTime.Now };
        var result = ConfigurationMapper.Map(entity);
        Assert.Equal(10, result.Id);
    }

    [Fact]
    public void Map_Entity_MapsSpecialistIdCorrectly()
    {
        var entity = new Configuration { Id = 1, SpecialistId = 5, CreatedAt = DateTime.Now };
        var result = ConfigurationMapper.Map(entity);
        Assert.Equal(5, result.SpecialistId);
    }

    [Fact]
    public void Map_Entity_MapsCreatedAtCorrectly()
    {
        var date = new DateTime(2024, 3, 15, 14, 30, 0);
        var entity = new Configuration { Id = 1, SpecialistId = 1, CreatedAt = date };
        var result = ConfigurationMapper.Map(entity);
        Assert.Equal(date, result.CreatedAt);
    }

    [Fact]
    public void Map_Entity_ReturnsConfigurationModel()
    {
        var entity = new Configuration { Id = 1, SpecialistId = 1, CreatedAt = DateTime.Now };
        var result = ConfigurationMapper.Map(entity);
        Assert.IsType<ConfigurationModel>(result);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(99, 3)]
    [InlineData(int.MaxValue, 1)]
    public void Map_Entity_VariousIds_MapsCorrectly(int configId, int specialistId)
    {
        var entity = new Configuration { Id = configId, SpecialistId = specialistId, CreatedAt = DateTime.Now };
        var result = ConfigurationMapper.Map(entity);

        Assert.Equal(configId, result.Id);
        Assert.Equal(specialistId, result.SpecialistId);
    }

    // -------------------------------------------------------------------------
    // Map: ConfigurationRequest → Configuration (entity)
    // -------------------------------------------------------------------------

    [Fact]
    public void Map_Request_MapsSpecialistIdCorrectly()
    {
        var request = new ConfigurationRequest { SpecialistId = 7, SelectedComponentIds = [] };
        var result = ConfigurationMapper.Map(request);
        Assert.Equal(7, result.SpecialistId);
    }

    [Fact]
    public void Map_Request_ReturnsConfigurationEntity()
    {
        var request = new ConfigurationRequest { SpecialistId = 1, SelectedComponentIds = [] };
        var result = ConfigurationMapper.Map(request);
        Assert.IsType<Configuration>(result);
    }

    [Fact]
    public void Map_Request_CreatedAt_IsSetToCurrentTime()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var request = new ConfigurationRequest { SpecialistId = 1, SelectedComponentIds = [] };

        var result = ConfigurationMapper.Map(request);

        var after = DateTime.Now.AddSeconds(1);
        Assert.InRange(result.CreatedAt, before, after);
    }

    [Fact]
    public void Map_Request_IdIsNotSetByMapper()
    {
        // Id is assigned by the DB on insert — mapper must leave it at default (0)
        var request = new ConfigurationRequest { SpecialistId = 3, SelectedComponentIds = [] };
        var result = ConfigurationMapper.Map(request);
        Assert.Equal(0, result.Id);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(999)]
    public void Map_Request_VariousSpecialistIds_MapsCorrectly(int specialistId)
    {
        var request = new ConfigurationRequest { SpecialistId = specialistId, SelectedComponentIds = [] };
        var result = ConfigurationMapper.Map(request);
        Assert.Equal(specialistId, result.SpecialistId);
    }
}
