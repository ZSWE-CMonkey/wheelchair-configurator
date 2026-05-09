namespace WheelchairConfigurator.ServiceLayer.Models;

/// <summary>
/// Represents the data sent from UI when creating a new configuration.
/// </summary>
public class ConfigurationRequest
{
    /// <summary>ID of the specialist creating the configuration.</summary>
    public int SpecialistId { get; set; }

    /// <summary>Patient data used for rules and calculations.</summary>
    public PatientProfileModel? Patient { get; set; }

    /// <summary>List of selected component IDs.</summary>
    public List<int> SelectedComponentIds { get; set; } = new();

    /// <summary>Patient identifier string stored with the configuration.</summary>
    public string PatientIdentificator { get; set; } = string.Empty;
}