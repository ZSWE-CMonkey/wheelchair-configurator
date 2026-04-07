namespace WheelchairConfigurator.ServiceLayer.Models;

/// <summary>
/// Defines the patient's trunk stability level. 
/// Used by the clinical engine to recommend specific support modules (e.g., lateral supports, tilt).
/// </summary>
public enum TrunkStabilityLevel
{
    /// <summary>Good - The patient sits independently without support.</summary>
    Good = 1,

    /// <summary>Fair - The patient requires light back or pelvic support.</summary>
    Fair = 2,

    /// <summary>Poor - The patient lacks balance and requires significant fixation (e.g., lateral supports, tilt module).</summary>
    Poor = 3
}