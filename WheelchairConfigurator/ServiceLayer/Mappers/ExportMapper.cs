using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.Export.ExportModel;

namespace WheelchairConfigurator.ServiceLayer.Mappers;

/// <summary>
/// Assembles a ConfigurationExportModel from database entities.
/// Called by AppService before delegating to IExportFileBuilder.
/// Optimized — loads all components and categories in bulk to avoid N+1 queries.
/// </summary>
public static class ExportMapper
{
    /// <summary>
    /// Loads all required data from repositories and builds the export model.
    /// </summary>
    public static async Task<ConfigurationExportModel> MapAsync(
        Configuration config,
        List<ConfigurationItem> items,
        Specialist specialist,
        ComponentRepository componentRepo,
        CategoryRepository categoryRepo)
    {
        var componentIds = items.Select(i => i.ComponentId).ToList();
        var components = await componentRepo.GetByIdsAsync(componentIds);
        var componentMap = components.ToDictionary(c => c.Id);

        var categoryIds = components.Select(c => c.CategoryId).Distinct().ToList();
        var categories = await categoryRepo.GetByIdsAsync(categoryIds);
        var categoryMap = categories.ToDictionary(c => c.Id);

        // Build export items
        var exportItems = items.Select(item =>
        {
            var component = componentMap[item.ComponentId];
            var category = categoryMap[component.CategoryId];

            return new ConfigurationExportItem
            {
                CategoryName = category.Name,
                ComponentName = component.Name,
                ItemCode = component.ItemCode ?? "N/A",
                Price = component.Price,
                Quantity = item.Quantity
            };
        }).ToList();

        return new ConfigurationExportModel
        {
            ConfigurationName = $"Configuration #{config.Id}",
            SpecialistName = specialist is not null
    ? $"{specialist.FirstName} {specialist.LastName}"
    : "Unknown Specialist",
            CreatedAt = config.CreatedAt,
            TotalPrice = exportItems.Sum(i => i.Price * i.Quantity),
            Items = exportItems
        };
    }
}