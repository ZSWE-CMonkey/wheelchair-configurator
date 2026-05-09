using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Interfaces;

/// <summary>
/// Main application service contract.
/// Handles all use-cases from the UI perspective.
/// UI communicates exclusively through this interface — never directly with repositories or domain entities.
/// </summary>
public interface IAppService
{
    /// <summary>
    /// Retrieves all available component categories.
    /// </summary>
    Task<List<CategoryModel>> GetCategoriesAsync();

    /// <summary>
    /// Retrieves components for a specific category.
    /// If a patient profile is provided, the engine flags each component
    /// as recommended or incompatible based on clinical rules.
    /// </summary>
    /// <param name="categoryId">ID of the category to load components from.</param>
    /// <param name="patient">Optional patient profile used for engine recommendations.</param>
    Task<List<ComponentModel>> GetComponentsAsync(int categoryId, PatientProfileModel? patient = null);

    /// <summary>
    /// Validates the current component selection against clinical compatibility rules.
    /// </summary>
    /// <param name="request">Contains selected component IDs and patient profile.</param>
    Task<ConfigurationResult> ValidateConfigurationAsync(ConfigurationRequest request);

    /// <summary>
    /// Saves the final configuration to the database.
    /// Runs validation before saving — returns error result if invalid.
    /// </summary>
    /// <param name="request">Contains specialist ID, selected component IDs and patient profile.</param>
    Task<ConfigurationResult> SaveConfigurationAsync(ConfigurationRequest request);

    /// <summary>
    /// Generates a PDF for the specified saved configuration.
    /// Returns the file path of the generated PDF.
    /// </summary>
    /// <param name="configurationId">ID of the configuration to export.</param>
    ///  Task<string> ExportConfigurationAsync(int configurationId);

    /// <summary>
    /// Retrieves all past configurations created by the specified specialist.
    /// </summary>
    /// <param name="specialistId">ID of the specialist.</param>
    Task<List<ConfigurationModel>> GetConfigurationsBySpecialistAsync(int specialistId);

    /// <summary>
    /// Generates a PDF for the specified saved configuration.
    /// Returns the file path of the generated PDF.
    /// </summary>
    /// <param name="configurationId">ID of the configuration to export.</param>
    Task<byte[]> ExportConfigurationAsync(int configurationId);

    /// <summary>
    /// Returns the components that belong to a saved configuration (for display in SummaryPage).
    /// </summary>
    Task<List<ComponentModel>> GetConfigurationComponentsAsync(int configurationId);

    /// <summary>
    /// Adds a new component to the catalog.
    /// </summary>
    Task<ConfigurationResult> AddComponentAsync(string name, int categoryId);

    /// <summary>
    /// Removes a component from the catalog.
    /// </summary>
    Task<ConfigurationResult> RemoveComponentAsync(int componentId);
}