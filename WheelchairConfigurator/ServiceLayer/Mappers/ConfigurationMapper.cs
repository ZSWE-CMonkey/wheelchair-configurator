using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Mappers;

/// <summary>
/// Maps between Configuration domain entity and ConfigurationModel.
/// Handles both directions — DB to UI and UI to DB.
/// </summary>
public static class ConfigurationMapper
{
    /// <summary>Converts a Configuration entity to a ConfigurationModel for UI.</summary>
    public static ConfigurationModel Map(Configuration entity) => new()
    {
        Id = entity.Id,
        SpecialistId = entity.SpecialistId,
        CreatedAt = entity.CreatedAt
    };

    /// <summary>Converts a ConfigurationRequest from UI to a Configuration entity for DB.</summary>
    public static Configuration Map(ConfigurationRequest request) => new()
    {
        SpecialistId = request.SpecialistId,
        CreatedAt = DateTime.Now
    };
}