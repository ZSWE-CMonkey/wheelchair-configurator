using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.Export.ExportModel;

namespace WheelchairConfigurator.ServiceLayer.Mappers;

public static class ExportMapper
{
    public static async Task<ConfigurationExportModel> MapAsync(
        Configuration config,
        List<ConfigurationItem> items,
        Specialist? specialist,
        IComponentRepository componentRepo,
        ICategoryRepository categoryRepo)
    {
        var componentIds = items.Select(i => i.ComponentId).ToList();
        var components = await componentRepo.GetByIdsAsync(componentIds);
        var componentMap = components.ToDictionary(c => c.Id);

        var categoryIds = components.Select(c => c.CategoryId).Distinct().ToList();
        var categories = await categoryRepo.GetByIdsAsync(categoryIds);
        var categoryMap = categories.ToDictionary(c => c.Id);

        var exportItems = items.Select(item =>
        {
            var component = componentMap[item.ComponentId];
            var category = categoryMap[component.CategoryId];

            return new ConfigurationExportItem
            {
                CategoryName = category.Name,
                ComponentName = component.Name,
                ItemCode = component.CatalogUrl ?? "-",
                Price = component.Price,
                Quantity = item.Quantity,
                Manufacturer = component.Manufacturer,
                ManufacturerCode = component.ManufacturerCode,
            };
        }).ToList();

        var specialistName = !string.IsNullOrEmpty(config.SpecialistName)
            ? config.SpecialistName
            : specialist is not null ? $"{specialist.FirstName} {specialist.LastName}" : "Neznámý terapeut";

        return new ConfigurationExportModel
        {
            ConfigurationName = $"Konfigurace #{config.Id} ({config.Hash[..8]})",
            SpecialistName = specialistName,
            PatientName = config.PatientName,
            PatientBirthNumber = config.PatientBirthNumber,
            CreatedAt = config.CreatedAt,
            TotalPrice = exportItems.Sum(i => i.Price * i.Quantity),
            Items = exportItems
        };
    }
}
