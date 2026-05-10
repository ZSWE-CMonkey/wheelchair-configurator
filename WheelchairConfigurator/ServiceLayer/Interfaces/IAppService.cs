using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Interfaces;

public interface IAppService
{
    // ── Categories ────────────────────────────────────────────────────────────

    Task<List<CategoryModel>> GetCategoriesAsync();

    // ── Components ────────────────────────────────────────────────────────────

    Task<List<ComponentModel>> GetComponentsAsync(int categoryId, PatientProfileModel? patient = null);
    Task<ConfigurationResult> AddComponentAsync(string name, int categoryId, string manufacturer = "", string manufacturerCode = "");
    Task<ConfigurationResult> UpdateComponentAsync(ComponentModel component);
    Task<ConfigurationResult> RemoveComponentAsync(int componentId);
    Task<byte[]> ExportComponentCatalogAsync();
    Task<ConfigurationResult> ImportComponentCatalogAsync(Stream jsonStream);

    // ── Configurations ────────────────────────────────────────────────────────

    Task<ConfigurationResult> ValidateConfigurationAsync(ConfigurationRequest request);
    Task<ConfigurationResult> SaveConfigurationAsync(ConfigurationRequest request);
    Task<List<ConfigurationModel>> GetConfigurationsBySpecialistAsync(int specialistId);
    Task<List<ComponentModel>> GetConfigurationComponentsAsync(int configurationId);
    Task<ConfigurationResult> CopyConfigurationAsync(int configurationId, int newSpecialistId, string newSpecialistName);
    Task<byte[]> ExportConfigurationAsync(int configurationId);

    // ── Specialists ───────────────────────────────────────────────────────────

    Task<List<SpecialistModel>> GetSpecialistsAsync();
    Task<SpecialistModel?> GetSpecialistByIdAsync(int specialistId);
    Task SaveSpecialistAsync(SpecialistModel model);
    Task DeactivateSpecialistAsync(int specialistId);

    // ── Patients ──────────────────────────────────────────────────────────────

    Task<List<PatientModel>> GetPatientsAsync();
    Task<PatientModel?> GetPatientByBirthNumberAsync(string birthNumber);
    Task<PatientModel> SavePatientAsync(PatientModel patient);
    Task DeactivatePatientAsync(int patientId);

    // ── Patient Measurements ──────────────────────────────────────────────────

    Task<List<PatientMeasurementModel>> GetMeasurementsForPatientAsync(int patientId);
    Task<PatientMeasurementModel> SaveMeasurementAsync(PatientMeasurementModel measurement);

    // ── Activity Log ──────────────────────────────────────────────────────────

    Task LogActivityAsync(string action, string entityType, int? entityId = null, string? detail = null);
    Task<List<ActivityLogModel>> GetActivityLogAsync(int pageSize = 100);

    // ── Settings ──────────────────────────────────────────────────────────────

    Task<AppSettingsModel> GetSettingsAsync();
    Task SaveSettingsAsync(AppSettingsModel settings);
}
