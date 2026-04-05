using WheelchairConfigurator.Data.DTOs;
using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.Domain.Models;
using ConfigurationLogic.DTOs;

namespace ConfigurationLogic;

// Catalog read operations
public class Catalog
{
    private readonly CategoryRepository _categoryRepository;
    private readonly ComponentRepository _componentRepository;
    private readonly ComponentSpecsRepository _componentSpecsRepository;

    // Initialize catalog repositories
    public Catalog(
        CategoryRepository categoryRepository,
        ComponentRepository componentRepository,
        ComponentSpecsRepository componentSpecsRepository)
    {
        _categoryRepository = categoryRepository;
        _componentRepository = componentRepository;
        _componentSpecsRepository = componentSpecsRepository;
    }

    // Load all categories
    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories
            .Select(c => new CategoryDto { Name = c.Name })
            .ToList();
    }

    // Load raw component entities
    public Task<List<Component>> GetAllComponentEntitiesAsync()
    {
        return _componentRepository.GetAllAsync();
    }

    // Load all component DTOs
    public async Task<List<ComponentDto>> GetAllComponentsAsync()
    {
        var components = await _componentRepository.GetAllAsync();
        return await MapComponentsToDtoAsync(components);
    }

    // Load components by category
    public async Task<List<ComponentDto>> GetComponentsByCategoryAsync(int categoryId)
    {
        var components = await _componentRepository.GetByCategoryIdAsync(categoryId);
        return await MapComponentsToDtoAsync(components);
    }

    // Load single component specs
    public async Task<ComponentSpecsDto?> GetComponentDetailAsync(int componentId)
    {
        var specs = await _componentSpecsRepository.GetByComponentIdAsync(componentId);
        if (specs is null)
        {
            return null;
        }

        var component = await _componentRepository.GetByIdAsync(componentId);
        return new ComponentSpecsDto
        {
            ComponentName = component?.Name ?? string.Empty,
            WeightCapacityKg = specs.WeightCapacityKg,
            SeatWidthCm = specs.SeatWidthCm,
            MaxSpeedKmh = specs.MaxSpeedKmh
        };
    }

    // Search components by name
    public async Task<List<ComponentDto>> SearchComponentsAsync(string? query)
    {
        var components = await _componentRepository.GetAllAsync();
        var trimmed = query?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(trimmed))
        {
            components = components
                .Where(c => c.Name.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return await MapComponentsToDtoAsync(components);
    }

    // Map entity to output DTO
    public async Task<ComponentOutputDto> ToComponentOutputDtoAsync(Component component)
    {
        var category = await _categoryRepository.GetByIdAsync(component.CategoryId);
        return new ComponentOutputDto
        {
            Id = component.Id,
            Name = component.Name,
            CategoryName = category?.Name ?? string.Empty,
            CatalogUrl = component.CatalogUrl,
            Price = component.Price
        };
    }

    // Map entity list to DTO list
    private async Task<List<ComponentDto>> MapComponentsToDtoAsync(List<Component> components)
    {
        var categories = await _categoryRepository.GetAllAsync();
        var categoryLookup = categories.ToDictionary(c => c.Id, c => c.Name);

        return components
            .Select(c => new ComponentDto
            {
                Name = c.Name,
                CategoryName = categoryLookup.GetValueOrDefault(c.CategoryId, string.Empty),
                CatalogUrl = c.CatalogUrl,
                Price = c.Price
            })
            .ToList();
    }
}
