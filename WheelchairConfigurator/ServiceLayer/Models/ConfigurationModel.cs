namespace WheelchairConfigurator.ServiceLayer.Models;

/// <summary>
/// UI model representing a saved configuration.
/// </summary>
public class ConfigurationModel
{
    /// <summary>Database ID of the configuration.</summary>
    public int Id { get; set; }

    /// <summary>ID of the specialist who created the configuration.</summary>
    public int SpecialistId { get; set; }

    /// <summary>Date and time when the configuration was created.</summary>
    public DateTime CreatedAt { get; set; }
}