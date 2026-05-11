using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Mappers;

/// <summary>
/// Maps between Component domain entity and ComponentModel.
/// IsRecommended and IsIncompatible are set by AppService after engine evaluation.
/// </summary>
public static class ComponentMapper
{
    /// <summary>Converts a Component entity to a ComponentModel for UI.</summary>
    public static ComponentModel Map(Component entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Price = entity.Price,
        CatalogUrl = entity.CatalogUrl,
        Manufacturer = entity.Manufacturer,
        ManufacturerCode = entity.ManufacturerCode,
    };
}