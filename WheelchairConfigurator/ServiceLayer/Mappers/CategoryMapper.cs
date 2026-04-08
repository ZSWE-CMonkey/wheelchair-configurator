using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Mappers;

/// <summary>
/// Maps between Category domain entity and CategoryModel.
/// </summary>
public static class CategoryMapper
{
    /// <summary>Converts a Category entity to a CategoryModel for UI.</summary>
    public static CategoryModel Map(Category entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name
    };
}