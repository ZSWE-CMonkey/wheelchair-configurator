using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.Data.DTOs;
using ConfigurationLogic.DTOs;

namespace ConfigurationLogic;

// Service facade
public class MainServices
{
    public Catalog Catalog { get; }
    public Configurator Configurator { get; }
    public Persistence Persistence { get; }

    // Build services from repositories
    public MainServices(
        CategoryRepository categoryRepository,
        ComponentRepository componentRepository,
        ComponentSpecsRepository componentSpecsRepository,
        CompatibilityRuleRepository compatibilityRuleRepository,
        ConfigurationRepository configurationRepository,
        ConfigurationItemRepository configurationItemRepository)
    {
        Catalog = new Catalog(categoryRepository, componentRepository, componentSpecsRepository);
        Configurator = new Configurator(Catalog, compatibilityRuleRepository);
        Persistence = new Persistence(configurationRepository, configurationItemRepository);
    }

    // Inject ready service instances
    public MainServices(Catalog catalog, Configurator configurator, Persistence persistence)
    {
        Catalog = catalog;
        Configurator = configurator;
        Persistence = persistence;
    }

    // Main frontend entrypoint for profile evaluation
    public Task<ProfileEvaluationResultDto> EvaluateProfileAsync(UserProfileDto profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return Configurator.EvaluateProfileAsync(profile);
    }

    // Build full component state for frontend (init + incremental updates)
    public async Task<ConfigurationStateResponseDto> EvaluateConfigurationStateAsync(ConfigurationStateRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Profile);

        var evaluation = await EvaluateProfileAsync(request.Profile);

        var eligibleByProfile = evaluation.EligibleComponents
            .Select(c => c.Id)
            .ToHashSet();

        var issuesByComponent = evaluation.Issues
            .GroupBy(i => i.ComponentId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Category).Distinct().ToList());

        var entities = await Catalog.GetAllComponentEntitiesAsync();
        var outputs = new List<ComponentOutputDto>(entities.Count);
        foreach (var entity in entities)
        {
            outputs.Add(await Catalog.ToComponentOutputDtoAsync(entity));
        }

        var outputById = outputs.ToDictionary(c => c.Id, c => c);

        // Hard fail: keep only profile-eligible components in selected set.
        // Soft fail: keep at most one selected component per category.
        var selectedIds = new HashSet<int>();
        var selectedCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var selectedId in request.SelectedComponentIds.Where(id => id > 0).Distinct())
        {
            if (!outputById.TryGetValue(selectedId, out var selectedOutput))
            {
                continue;
            }

            if (!eligibleByProfile.Contains(selectedId))
            {
                continue;
            }

            if (selectedCategories.Contains(selectedOutput.CategoryName))
            {
                continue;
            }

            selectedIds.Add(selectedId);
            selectedCategories.Add(selectedOutput.CategoryName);
        }

        var selectedCategoryById = selectedIds
            .Where(id => outputById.ContainsKey(id))
            .ToDictionary(id => id, id => outputById[id].CategoryName);

        var states = new List<ComponentStateDto>(outputs.Count);

        foreach (var component in outputs)
        {
            var isSelected = selectedIds.Contains(component.Id);
            var isEnabled = eligibleByProfile.Contains(component.Id);
            var disableReasons = new List<string>();

            if (!isEnabled)
            {
                if (issuesByComponent.TryGetValue(component.Id, out var categories) && categories.Count > 0)
                {
                    disableReasons.AddRange(categories.Select(c => $"Komponenta nevyhovuje profilu uživatele (pravidlo: {c})."));
                }
                else
                {
                    disableReasons.Add("Komponenta nevyhovuje profilu uživatele.");
                }
            }

            // Allow only one selected component per category.
            if (isEnabled)
            {
                var hasDifferentSelectedInCategory = selectedCategoryById
                    .Any(x => x.Value == component.CategoryName && x.Key != component.Id);

                if (hasDifferentSelectedInCategory)
                {
                    isEnabled = false;
                    disableReasons.Add($"V kategorii '{component.CategoryName}' už je vybraná jiná komponenta.");
                }
            }

            // Unknown compatibility rule (null) is treated as compatible for now.
            if (isEnabled)
            {
                foreach (var selectedId in selectedIds)
                {
                    if (selectedId == component.Id)
                    {
                        continue;
                    }

                    var compatible = await CheckCompatibilityAsync(component.Id, selectedId);
                    if (compatible == false)
                    {
                        isEnabled = false;
                        var conflictingName = outputById.TryGetValue(selectedId, out var selectedComponent)
                            ? selectedComponent.Name
                            : $"ID {selectedId}";
                        disableReasons.Add($"Komponenta není kompatibilní s vybranou komponentou '{conflictingName}'.");
                        break;
                    }
                }
            }

            states.Add(new ComponentStateDto
            {
                Component = component,
                IsSelected = isSelected,
                IsEnabled = isEnabled,
                DisableReasons = disableReasons
            });
        }

        return new ConfigurationStateResponseDto
        {
            Requirements = evaluation.Requirements,
            Issues = evaluation.Issues,
            Recommendations = evaluation.Recommendations,
            EligibleComponentIds = eligibleByProfile.OrderBy(id => id).ToList(),
            SelectedComponentIds = selectedIds.OrderBy(id => id).ToList(),
            Components = states
        };
    }

    // Frontend-friendly init call with empty selection
    public Task<ConfigurationStateResponseDto> InitializeConfigurationAsync(UserProfileDto profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return EvaluateConfigurationStateAsync(new ConfigurationStateRequestDto
        {
            Profile = profile,
            SelectedComponentIds = new List<int>()
        });
    }

    // Frontend-friendly refresh call with explicit selected ids
    public Task<ConfigurationStateResponseDto> RefreshConfigurationAsync(UserProfileDto profile, IEnumerable<int> selectedComponentIds)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(selectedComponentIds);

        return EvaluateConfigurationStateAsync(new ConfigurationStateRequestDto
        {
            Profile = profile,
            SelectedComponentIds = selectedComponentIds.ToList()
        });
    }

    // Apply one frontend click (select/deselect) and return refreshed state
    public async Task<ConfigurationStateResponseDto> ToggleComponentSelectionAsync(
        UserProfileDto profile,
        IEnumerable<int> currentSelectedComponentIds,
        int clickedComponentId)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(currentSelectedComponentIds);

        var selected = currentSelectedComponentIds
            .Where(id => id > 0)
            .Distinct()
            .ToHashSet();

        // Toggle off if already selected.
        if (selected.Contains(clickedComponentId))
        {
            selected.Remove(clickedComponentId);
        }
        else
        {
            selected.Add(clickedComponentId);

            // Keep one selected item per category.
            var entities = await Catalog.GetAllComponentEntitiesAsync();
            var componentById = entities.ToDictionary(c => c.Id, c => c);

            if (componentById.TryGetValue(clickedComponentId, out var clickedEntity))
            {
                var sameCategoryIds = selected
                    .Where(id => id != clickedComponentId)
                    .Where(id => componentById.TryGetValue(id, out var candidate) && candidate.CategoryId == clickedEntity.CategoryId)
                    .ToList();

                foreach (var sameCategoryId in sameCategoryIds)
                {
                    selected.Remove(sameCategoryId);
                }
            }
        }

        return await EvaluateConfigurationStateAsync(new ConfigurationStateRequestDto
        {
            Profile = profile,
            SelectedComponentIds = selected.ToList()
        });
    }

    // Return only eligible components from evaluation
    public async Task<List<ComponentOutputDto>> GetEligibleComponentsAsync(UserProfileDto profile)
    {
        var evaluation = await EvaluateProfileAsync(profile);
        return evaluation.EligibleComponents;
    }

    // Save explicit component selection from frontend
    public Task<int> SaveConfigurationAsync(int specialistId, IEnumerable<int> componentIds)
    {
        ArgumentNullException.ThrowIfNull(componentIds);
        return Persistence.SaveConfigurationAsync(specialistId, componentIds);
    }

    // Evaluate profile and persist top eligible components
    public async Task<int> EvaluateAndSaveConfigurationAsync(int specialistId, UserProfileDto profile, int? maxComponentCount = null)
    {
        var evaluation = await EvaluateProfileAsync(profile);

        var selectedIds = evaluation.EligibleComponents
            .Select(c => c.Id)
            .Where(id => id > 0);

        if (maxComponentCount.HasValue && maxComponentCount.Value > 0)
        {
            selectedIds = selectedIds.Take(maxComponentCount.Value);
        }

        var componentIds = selectedIds.ToList();
        if (componentIds.Count == 0)
        {
            throw new InvalidOperationException("Pro zadaný profil nebyly nalezeny žádné vhodné komponenty.");
        }

        return await Persistence.SaveConfigurationAsync(specialistId, componentIds);
    }

    // Validate pair compatibility
    public Task<bool?> CheckCompatibilityAsync(int componentAId, int componentBId)
    {
        return Configurator.CheckCompatibilityAsync(componentAId, componentBId);
    }

    // Catalog passthrough for frontend listing
    public Task<List<CategoryDto>> GetCategoriesAsync() => Catalog.GetCategoriesAsync();

    // Catalog passthrough for frontend listing
    public Task<List<ComponentDto>> GetAllComponentsAsync() => Catalog.GetAllComponentsAsync();

    // Catalog passthrough for frontend filters
    public Task<List<ComponentDto>> GetComponentsByCategoryAsync(int categoryId) => Catalog.GetComponentsByCategoryAsync(categoryId);

    // Catalog passthrough for frontend search
    public Task<List<ComponentDto>> SearchComponentsAsync(string? query) => Catalog.SearchComponentsAsync(query);

    // Catalog passthrough for component detail
    public async Task<ComponentSpecsOutputDto?> GetComponentDetailAsync(int componentId)
    {
        var specs = await Catalog.GetComponentDetailAsync(componentId);
        if (specs is null)
        {
            return null;
        }

        return new ComponentSpecsOutputDto
        {
            ComponentId = componentId,
            ComponentName = specs.ComponentName,
            WeightCapacityKg = specs.WeightCapacityKg,
            SeatWidthCm = specs.SeatWidthCm,
            MaxSpeedKmh = specs.MaxSpeedKmh
        };
    }

    // Load saved configurations for specialist
    public Task<List<WheelchairConfigurator.Domain.Models.Configuration>> GetConfigurationsBySpecialistAsync(int specialistId)
    {
        return Persistence.GetConfigurationsBySpecialistAsync(specialistId);
    }

    // Load items for one saved configuration
    public Task<List<WheelchairConfigurator.Domain.Models.ConfigurationItem>> GetConfigurationItemsAsync(int configurationId)
    {
        return Persistence.GetConfigurationItemsAsync(configurationId);
    }
}
