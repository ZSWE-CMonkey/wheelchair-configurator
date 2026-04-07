using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.ConfigurationLogic;

/// <summary>
/// Temporary mock implementation of IConfigurationEngine.
/// Used for integration testing until the real engine is implemented.
/// Replace with real engine logic when ready.
/// </summary>
public class MockEngine : IConfigurationEngine
{
    /// <inheritdoc/>
    public Task<List<int>> GetRecommendedComponentIdsAsync(
        PatientProfileModel patient,
        List<ComponentModel> availableComponents)
        => Task.FromResult(availableComponents.Select(c => c.Id).ToList());

    /// <inheritdoc/>
    public Task<ConfigurationResult> ValidateAsync(
        ConfigurationRequest request,
        List<ComponentModel> selectedComponents)
        => Task.FromResult(new ConfigurationResult
        {
            IsSuccess = true,
            Message = "[MockEngine] Validation passed."
        });
}