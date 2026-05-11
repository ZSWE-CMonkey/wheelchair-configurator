using System.Text;
using System.Text.Json;
using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.Export;
using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Mappers;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer;

public class AppService : IAppService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IComponentRepository _componentRepo;
    private readonly IConfigurationRepository _configurationRepo;
    private readonly IConfigurationItemRepository _configurationItemRepo;
    private readonly ISpecialistRepository _specialistRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IPatientMeasurementRepository _measurementRepo;
    private readonly IActivityLogRepository _activityLogRepo;
    private readonly IAppSettingRepository _settingRepo;
    private readonly IConfigurationEngine _engine;
    private readonly IExportFileBuilder _fileBuilder;
    private readonly Model3DRepository _model3DRepo;

    public AppService(
        ICategoryRepository categoryRepo,
        IComponentRepository componentRepo,
        IConfigurationRepository configurationRepo,
        IConfigurationItemRepository configurationItemRepo,
        ISpecialistRepository specialistRepo,
        IPatientRepository patientRepo,
        IPatientMeasurementRepository measurementRepo,
        IActivityLogRepository activityLogRepo,
        IAppSettingRepository settingRepo,
        IConfigurationEngine engine,
        IExportFileBuilder fileBuilder,
        Model3DRepository model3DRepo)
    {
        _categoryRepo = categoryRepo;
        _componentRepo = componentRepo;
        _configurationRepo = configurationRepo;
        _configurationItemRepo = configurationItemRepo;
        _specialistRepo = specialistRepo;
        _patientRepo = patientRepo;
        _measurementRepo = measurementRepo;
        _activityLogRepo = activityLogRepo;
        _settingRepo = settingRepo;
        _engine = engine;
        _fileBuilder = fileBuilder;
        _model3DRepo = model3DRepo;
    }

    // ── Categories ────────────────────────────────────────────────────────────

    public async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return categories.Select(CategoryMapper.Map).ToList();
    }

    // ── Components ────────────────────────────────────────────────────────────

    public async Task<List<ComponentModel>> GetComponentsAsync(int categoryId, PatientProfileModel? patient = null)
    {
        var components = await _componentRepo.GetByCategoryIdAsync(categoryId);
        var componentModels = components.Select(ComponentMapper.Map).ToList();

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

    public async Task<ConfigurationResult> AddComponentAsync(string name, int categoryId, string manufacturer = "", string manufacturerCode = "", string catalogUrl = "")
    {
        await _componentRepo.InsertAsync(new Component
        {
            Name = name,
            CategoryId = categoryId,
            Price = 0,
            Manufacturer = manufacturer,
            ManufacturerCode = manufacturerCode,
            CatalogUrl = string.IsNullOrWhiteSpace(catalogUrl) ? null : catalogUrl,
        });
        return new ConfigurationResult { IsSuccess = true, Message = "Komponenta přidána." };
    }

    public async Task<ConfigurationResult> UpdateComponentAsync(ComponentModel model)
    {
        var entity = await _componentRepo.GetByIdAsync(model.Id);
        if (entity is null)
            return new ConfigurationResult { IsSuccess = false, Message = "Komponenta nenalezena." };

        entity.Name = model.Name;
        entity.Price = model.Price;
        entity.CatalogUrl = model.CatalogUrl;
        entity.Manufacturer = model.Manufacturer;
        entity.ManufacturerCode = model.ManufacturerCode;

        await _componentRepo.UpdateAsync(entity);
        return new ConfigurationResult { IsSuccess = true, Message = "Komponenta aktualizována." };
    }

    public async Task<ConfigurationResult> RemoveComponentAsync(int componentId)
    {
        var component = await _componentRepo.GetByIdAsync(componentId);
        if (component is null)
            return new ConfigurationResult { IsSuccess = false, Message = "Komponenta nenalezena." };
        await _componentRepo.DeleteAsync(component);
        return new ConfigurationResult { IsSuccess = true, Message = "Komponenta odstraněna." };
    }

    public async Task<byte[]> ExportComponentCatalogAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

        var allComponents = new List<Component>();
        foreach (var cat in categories)
        {
            var comps = await _componentRepo.GetByCategoryIdAsync(cat.Id);
            allComponents.AddRange(comps);
        }

        var export = allComponents.Select(c => new
        {
            c.Id,
            c.Name,
            CategoryName = categoryMap.TryGetValue(c.CategoryId, out var cn) ? cn : string.Empty,
            c.Price,
            c.Manufacturer,
            c.ManufacturerCode,
            c.CatalogUrl,
        });

        var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
        return Encoding.UTF8.GetBytes(json);
    }

    public async Task<ConfigurationResult> ImportComponentCatalogAsync(Stream jsonStream)
    {
        try
        {
            using var reader = new StreamReader(jsonStream, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();
            var items = JsonSerializer.Deserialize<List<JsonElement>>(json);
            if (items is null)
                return new ConfigurationResult { IsSuccess = false, Message = "Neplatný formát souboru." };

            var categories = await _categoryRepo.GetAllAsync();
            var categoryMap = categories.ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);

            int imported = 0;
            foreach (var item in items)
            {
                var name = item.TryGetProperty("Name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
                var categoryName = item.TryGetProperty("CategoryName", out var cn) ? cn.GetString() ?? string.Empty : string.Empty;
                if (string.IsNullOrWhiteSpace(name) || !categoryMap.TryGetValue(categoryName, out var categoryId))
                    continue;

                var manufacturer = item.TryGetProperty("Manufacturer", out var mf) ? mf.GetString() ?? string.Empty : string.Empty;
                var manufacturerCode = item.TryGetProperty("ManufacturerCode", out var mc) ? mc.GetString() ?? string.Empty : string.Empty;
                var catalogUrl = item.TryGetProperty("CatalogUrl", out var cu) ? cu.GetString() : null;
                var price = item.TryGetProperty("Price", out var p) ? p.GetDecimal() : 0m;

                await _componentRepo.InsertAsync(new Component
                {
                    Name = name,
                    CategoryId = categoryId,
                    Price = price,
                    Manufacturer = manufacturer,
                    ManufacturerCode = manufacturerCode,
                    CatalogUrl = catalogUrl,
                });
                imported++;
            }

            return new ConfigurationResult { IsSuccess = true, Message = $"Importováno {imported} komponent." };
        }
        catch (Exception ex)
        {
            return new ConfigurationResult { IsSuccess = false, Message = $"Chyba při importu: {ex.Message}" };
        }
    }

    // ── Configurations ────────────────────────────────────────────────────────

    public async Task<ConfigurationResult> ValidateConfigurationAsync(ConfigurationRequest request)
    {
        var components = await _componentRepo.GetByIdsAsync(request.SelectedComponentIds);
        var selectedComponents = components.Select(ComponentMapper.Map).ToList();
        return await _engine.ValidateAsync(request, selectedComponents);
    }

    public async Task<ConfigurationResult> SaveConfigurationAsync(ConfigurationRequest request)
    {
        var validationResult = await ValidateConfigurationAsync(request);
        if (!validationResult.IsSuccess)
            return validationResult;

        var configuration = ConfigurationMapper.Map(request);
        await _configurationRepo.InsertAsync(configuration);

        foreach (var componentId in request.SelectedComponentIds)
        {
            await _configurationItemRepo.InsertAsync(new ConfigurationItem
            {
                ConfigurationId = configuration.Id,
                ComponentId = componentId,
                Quantity = 1
            });
        }

        await LogActivityAsync("Uložení konfigurace", "Configuration", configuration.Id,
            $"Pacient: {request.PatientName}, Hash: {configuration.Hash}");

        return new ConfigurationResult
        {
            IsSuccess = true,
            Message = "Konfigurace uložena.",
            ConfigurationId = configuration.Id
        };
    }

    public async Task<ConfigurationModel?> GetConfigurationAsync(int configurationId)
    {
        var entity = await _configurationRepo.GetByIdAsync(configurationId);
        return entity is null ? null : ConfigurationMapper.Map(entity);
    }

    public async Task<List<ConfigurationModel>> GetConfigurationsBySpecialistAsync(int specialistId)
    {
        var configurations = await _configurationRepo.GetBySpecialistIdAsync(specialistId);
        return configurations.Select(ConfigurationMapper.Map).ToList();
    }

    public async Task<List<ComponentModel>> GetConfigurationComponentsAsync(int configurationId)
    {
        var items = await _configurationItemRepo.GetByConfigurationIdAsync(configurationId);
        var ids = items.Select(i => i.ComponentId).ToList();
        var components = await _componentRepo.GetByIdsAsync(ids);
        return components.Select(ComponentMapper.Map).ToList();
    }

    public async Task<ConfigurationResult> CopyConfigurationAsync(int configurationId, int newSpecialistId, string newSpecialistName)
    {
        var original = await _configurationRepo.GetByIdAsync(configurationId);
        if (original is null)
            return new ConfigurationResult { IsSuccess = false, Message = "Konfigurace nenalezena." };

        var copy = new Configuration
        {
            SpecialistId = newSpecialistId,
            SpecialistName = newSpecialistName,
            CreatedAt = DateTime.Now,
            PatientMeasurementId = original.PatientMeasurementId,
            PatientBirthNumber = original.PatientBirthNumber,
            PatientName = original.PatientName,
            Hash = Guid.NewGuid().ToString("N"),
        };
        await _configurationRepo.InsertAsync(copy);

        var items = await _configurationItemRepo.GetByConfigurationIdAsync(configurationId);
        foreach (var item in items)
        {
            await _configurationItemRepo.InsertAsync(new ConfigurationItem
            {
                ConfigurationId = copy.Id,
                ComponentId = item.ComponentId,
                Quantity = item.Quantity
            });
        }

        await LogActivityAsync("Kopírování konfigurace", "Configuration", copy.Id,
            $"Zkopírováno z #{configurationId}");

        return new ConfigurationResult { IsSuccess = true, Message = "Konfigurace zkopírována.", ConfigurationId = copy.Id };
    }

    public async Task<byte[]> ExportConfigurationAsync(int configurationId)
    {
        var config = await _configurationRepo.GetByIdAsync(configurationId);
        var items = await _configurationItemRepo.GetByConfigurationIdAsync(configurationId);
        var specialist = config is not null ? await _specialistRepo.GetByIdAsync(config.SpecialistId) : null;

        var exportModel = await ExportMapper.MapAsync(config!, items, specialist, _componentRepo, _categoryRepo);
        return _fileBuilder.Build(exportModel);
    }

    // ── Specialists ───────────────────────────────────────────────────────────

    public async Task<List<SpecialistModel>> GetSpecialistsAsync()
    {
        var specialists = await _specialistRepo.GetAllActiveAsync();
        return specialists.Select(SpecialistMapper.Map).ToList();
    }

    public async Task<SpecialistModel?> GetSpecialistByIdAsync(int specialistId)
    {
        var entity = await _specialistRepo.GetByIdAsync(specialistId);
        return entity is null ? null : SpecialistMapper.Map(entity);
    }

    public async Task SaveSpecialistAsync(SpecialistModel model)
    {
        if (model.Id == 0)
        {
            var entity = SpecialistMapper.Map(model);
            entity.CreatedAt = DateTime.Now;
            await _specialistRepo.InsertAsync(entity);
            await LogActivityAsync("Přidání terapeuta", "Specialist", entity.Id, model.FullName);
        }
        else
        {
            var existing = await _specialistRepo.GetByIdAsync(model.Id);
            if (existing is null) return;
            existing.FirstName = model.FirstName;
            existing.LastName = model.LastName;
            existing.Email = model.Email;
            existing.Clinic = model.Clinic;
            await _specialistRepo.UpdateAsync(existing);
            await LogActivityAsync("Úprava terapeuta", "Specialist", model.Id, model.FullName);
        }
    }

    public async Task DeactivateSpecialistAsync(int specialistId)
    {
        var entity = await _specialistRepo.GetByIdAsync(specialistId);
        if (entity is null) return;
        entity.IsActive = false;
        await _specialistRepo.UpdateAsync(entity);
        await LogActivityAsync("Deaktivace terapeuta", "Specialist", specialistId);
    }

    // ── Patients ──────────────────────────────────────────────────────────────

    public async Task<List<PatientModel>> GetPatientsAsync()
    {
        var patients = await _patientRepo.GetAllActiveAsync();
        return patients.Select(PatientMapper.Map).ToList();
    }

    public async Task<PatientModel?> GetPatientByBirthNumberAsync(string birthNumber)
    {
        var entity = await _patientRepo.GetByBirthNumberAsync(birthNumber);
        return entity is null ? null : PatientMapper.Map(entity);
    }

    public async Task<PatientModel> SavePatientAsync(PatientModel model)
    {
        var existing = await _patientRepo.GetByBirthNumberAsync(model.BirthNumber);
        if (existing is null)
        {
            var entity = PatientMapper.Map(model);
            entity.CreatedAt = DateTime.Now;
            await _patientRepo.InsertAsync(entity);
            model.Id = entity.Id;
            await LogActivityAsync("Přidání pacienta", "Patient", entity.Id, model.FullName);
        }
        else
        {
            existing.FirstName = model.FirstName;
            existing.LastName = model.LastName;
            existing.IsActive = model.IsActive;
            await _patientRepo.UpdateAsync(existing);
            model.Id = existing.Id;
            await LogActivityAsync("Úprava pacienta", "Patient", existing.Id, model.FullName);
        }
        return model;
    }

    public async Task DeactivatePatientAsync(int patientId)
    {
        var entity = await _patientRepo.GetByIdAsync(patientId);
        if (entity is null) return;
        entity.IsActive = false;
        await _patientRepo.UpdateAsync(entity);
        await LogActivityAsync("Deaktivace pacienta", "Patient", patientId);
    }

    // ── Patient Measurements ──────────────────────────────────────────────────

    public async Task<List<PatientMeasurementModel>> GetMeasurementsForPatientAsync(int patientId)
    {
        var patient = await _patientRepo.GetByIdAsync(patientId);
        var measurements = await _measurementRepo.GetByPatientIdAsync(patientId);
        return measurements.Select(m => PatientMeasurementMapper.Map(m, patient)).ToList();
    }

    public async Task<PatientMeasurementModel?> GetMeasurementByIdAsync(int measurementId)
    {
        var entity = await _measurementRepo.GetByIdAsync(measurementId);
        if (entity is null) return null;
        var patient = await _patientRepo.GetByIdAsync(entity.PatientId);
        return PatientMeasurementMapper.Map(entity, patient);
    }

    public async Task<PatientMeasurementModel> SaveMeasurementAsync(PatientMeasurementModel model)
    {
        var entity = PatientMeasurementMapper.Map(model);
        entity.MeasuredAt = DateTime.Now;
        await _measurementRepo.InsertAsync(entity);
        model.Id = entity.Id;

        var patient = await _patientRepo.GetByIdAsync(model.PatientId);
        await LogActivityAsync("Přidání měření", "PatientMeasurement", entity.Id,
            $"Pacient: {patient?.LastName} {patient?.FirstName}");
        return model;
    }

    // ── Activity Log ──────────────────────────────────────────────────────────

    public async Task LogActivityAsync(string action, string entityType, int? entityId = null, string? detail = null)
    {
        await _activityLogRepo.InsertAsync(new ActivityLog
        {
            OccurredAt = DateTime.Now,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Detail = detail,
        });
    }

    public async Task<List<ActivityLogModel>> GetActivityLogAsync(int pageSize = 100)
    {
        var logs = await _activityLogRepo.GetRecentAsync(pageSize);
        return logs.Select(ActivityLogMapper.Map).ToList();
    }

    // ── Settings ──────────────────────────────────────────────────────────────

    public async Task<AppSettingsModel> GetSettingsAsync()
    {
        var renderingVal = await _settingRepo.GetAsync("RenderingEnabled");
        return new AppSettingsModel
        {
            RenderingEnabled = renderingVal is null || renderingVal == "true",
        };
    }

    public async Task SaveSettingsAsync(AppSettingsModel settings)
    {
        await _settingRepo.SetAsync("RenderingEnabled", settings.RenderingEnabled ? "true" : "false");
    }

    // ── 3D Models ─────────────────────────────────────────────────────────────

    public async Task<List<Model3DModel>> GetAllModel3DsAsync()
    {
        var models = await _model3DRepo.GetAllAsync();
        return models.Select(m => new Model3DModel
        {
            ComponentId = m.ComponentId,
            FilePath = m.FilePath,
            TextureId = m.TextureId
        }).ToList();
    }
}
