using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.Export;
using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Mappers;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer;

/// <summary>
/// Main orchestrator of the application logic.
/// Connects UI, DataLayer, ConfigurationEngine and ExportLayer.
/// UI communicates exclusively through IAppService — never directly with repositories.
/// AppService contains no business logic — it only orchestrates calls between layers.
/// </summary>
public class AppService : IAppService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IComponentRepository _componentRepo;
    private readonly IConfigurationRepository _configurationRepo;
    private readonly IConfigurationItemRepository _configurationItemRepo;
    private readonly ISpecialistRepository _specialistRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IConfigurationEngine _engine;
    private readonly IExportFileBuilder _fileBuilder;

    public AppService(
        ICategoryRepository categoryRepo,
        IComponentRepository componentRepo,
        IConfigurationRepository configurationRepo,
        IConfigurationItemRepository configurationItemRepo,
        ISpecialistRepository specialistRepo,
        IPatientRepository patientRepo,
        IConfigurationEngine engine,
        IExportFileBuilder fileBuilder)
    {
        _categoryRepo = categoryRepo;
        _componentRepo = componentRepo;
        _configurationRepo = configurationRepo;
        _configurationItemRepo = configurationItemRepo;
        _specialistRepo = specialistRepo;
        _patientRepo = patientRepo;
        _engine = engine;
        _fileBuilder = fileBuilder;
    }
    /// <inheritdoc/>
    public async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return categories.Select(CategoryMapper.Map).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ComponentModel>> GetComponentsAsync(int categoryId, PatientProfileModel? patient = null)
    {
        // 1. Load components from DB
        var components = await _componentRepo.GetByCategoryIdAsync(categoryId);

        // 2. Map to UI models
        var componentModels = components.Select(ComponentMapper.Map).ToList();

        // 3. If patient profile provided, ask engine to flag recommended/incompatible
        if (patient is not null)
        {
            var recommendedIds = await _engine.GetRecommendedComponentIdsAsync(patient, componentModels);

            foreach (var component in componentModels)
            {
                component.IsRecommended = recommendedIds.Contains(component.Id);
                component.IsIncompatible = !recommendedIds.Contains(component.Id);
            }
        }

        return componentModels;
    }

    /// <inheritdoc/>
    public async Task<ConfigurationResult> ValidateConfigurationAsync(ConfigurationRequest request)
    {
        var components = await _componentRepo.GetByIdsAsync(request.SelectedComponentIds);
        var selectedComponents = components.Select(ComponentMapper.Map).ToList();

        return await _engine.ValidateAsync(request, selectedComponents);
    }

    /// <inheritdoc/>
    public async Task<ConfigurationResult> SaveConfigurationAsync(ConfigurationRequest request)
    {
        // 1. Validate first
        var validationResult = await ValidateConfigurationAsync(request);
        if (!validationResult.IsSuccess)
            return validationResult;

        // 2. Map request to entity — via mapper
        var configuration = ConfigurationMapper.Map(request);
        await _configurationRepo.InsertAsync(configuration);

        // 3. Save configuration items
        foreach (var componentId in request.SelectedComponentIds)
        {
            await _configurationItemRepo.InsertAsync(new ConfigurationItem
            {
                ConfigurationId = configuration.Id,
                ComponentId = componentId,
                Quantity = 1
            });
        }

        return new ConfigurationResult
        {
            IsSuccess = true,
            Message = "Configuration saved successfully.",
            ConfigurationId = configuration.Id
        };
    }

    /// <inheritdoc/>
    public async Task<byte[]> ExportConfigurationAsync(int configurationId)
    {
        // 1. Load configuration from DB
        var config = await _configurationRepo.GetByIdAsync(configurationId);
        var items = await _configurationItemRepo.GetByConfigurationIdAsync(configurationId);
        var specialist = await _specialistRepo.GetByIdAsync(config!.SpecialistId);

        // 2. Build export model via mapper
        var exportModel = await ExportMapper.MapAsync(
            config,
            items,
            specialist!,
            _componentRepo,
            _categoryRepo
        );

        // 3. Build PDF directly
        return _fileBuilder.Build(exportModel);
    }

    /// <inheritdoc/>
    public async Task<List<ConfigurationModel>> GetConfigurationsBySpecialistAsync(int specialistId)
    {
        var configurations = await _configurationRepo.GetBySpecialistIdAsync(specialistId);
        return configurations.Select(ConfigurationMapper.Map).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ComponentModel>> GetConfigurationComponentsAsync(int configurationId)
    {
        var items = await _configurationItemRepo.GetByConfigurationIdAsync(configurationId);
        var ids = items.Select(i => i.ComponentId).ToList();
        var components = await _componentRepo.GetByIdsAsync(ids);
        return components.Select(ComponentMapper.Map).ToList();
    }

    /// <inheritdoc/>
    public async Task<ConfigurationResult> AddComponentAsync(string name, int categoryId)
    {
        await _componentRepo.InsertAsync(new WheelchairConfigurator.Domain.Models.Component
        {
            Name = name,
            CategoryId = categoryId,
            Price = 0
        });
        return new ConfigurationResult { IsSuccess = true, Message = "Komponenta přidána." };
    }

    /// <inheritdoc/>
    public async Task<ConfigurationResult> RemoveComponentAsync(int componentId)
    {
        var component = await _componentRepo.GetByIdAsync(componentId);
        if (component is null)
            return new ConfigurationResult { IsSuccess = false, Message = "Komponenta nenalezena." };
        await _componentRepo.DeleteAsync(component);
        return new ConfigurationResult { IsSuccess = true, Message = "Komponenta odstraněna." };
    }

    /// <inheritdoc/>
    public async Task SavePatientAsync(PatientModel model)
    {
        var existing = await _patientRepo.GetByIdentificatorAsync(model.PatientIdentificator, model.SpecialistId);
        if (existing is null)
        {
            await _patientRepo.InsertAsync(PatientMapper.Map(model));
        }
        else
        {
            var updated = PatientMapper.Map(model);
            updated.Id = existing.Id;
            updated.CreatedAt = existing.CreatedAt;
            await _patientRepo.UpdateAsync(updated);
        }
    }

    /// <inheritdoc/>
    public async Task<PatientModel?> GetPatientByIdentificatorAsync(string patientIdentificator, int specialistId = 1)
    {
        var entity = await _patientRepo.GetByIdentificatorAsync(patientIdentificator, specialistId);
        return entity is null ? null : PatientMapper.Map(entity);
    }
}