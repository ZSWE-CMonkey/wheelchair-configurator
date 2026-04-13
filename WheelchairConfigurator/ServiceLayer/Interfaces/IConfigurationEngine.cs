using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ServiceLayer.Interfaces;

/// <summary>
/// Defines the contract for the configuration engine.
/// Implemented by the ConfigurationLogic project.
/// AppService calls this interface — never the implementation directly.
/// </summary>
public interface IConfigurationEngine
{
    /// <summary>
    /// Recommends components based on the patient's profile and physical dimensions.
    /// Filters out components that don't match weight limits or clinical requirements.
    /// </summary>
    /// <param name="patient">The patient profile containing anthropometric and clinical data.</param>
    /// <param name="availableComponents">List of all components in a specific category to be evaluated.</param>
    /// <returns>A list of component IDs that are safe and recommended for the patient.</returns>
    Task<List<int>> GetRecommendedComponentIdsAsync(PatientProfileModel patient, List<ComponentModel> availableComponents);

    /// <summary>
    /// Validates selected components against clinical rules, weight capacities, and inter-compatibility.
    /// </summary>
    /// <param name="request">The configuration request containing selected component IDs and patient data.</param>
    /// <param name="selectedComponentsFullData">Full data of the selected components so the engine can check dimensions and capacities.</param>
    /// <returns>Validation result indicating success or specific errors.</returns>
    Task<ConfigurationResult> ValidateAsync(ConfigurationRequest request, List<ComponentModel> selectedComponentsFullData);
}