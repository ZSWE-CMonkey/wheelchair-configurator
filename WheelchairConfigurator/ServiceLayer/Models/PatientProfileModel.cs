namespace WheelchairConfigurator.ServiceLayer.Models;

/// <summary>
/// Represents the patient's anthropometric and clinical data.
/// Sent from the UI so the Engine can calculate dimensions and validate weight capacities.
/// </summary>
public class PatientProfileModel
{
    // --- ANTHROPOMETRY (Dimensions) ---

    /// <summary>Pelvis width in centimeters (determines the seat width).</summary>
    public int PelvisWidthCm { get; set; }

    /// <summary>Thigh length from the buttocks to the popliteal fossa in centimeters (determines the seat depth).</summary>
    public int ThighLengthCm { get; set; }

    /// <summary>Lower leg length from the popliteal fossa to the heel in centimeters (determines the footrest length).</summary>
    public int LowerLegLengthCm { get; set; }


    // --- PHYSICAL AND CLINICAL CONDITIONS ---

    /// <summary>Patient weight in kilograms (crucial for chassis and wheel weight capacity).</summary>
    public int WeightKg { get; set; }

    /// <summary>Patient's trunk stability level.</summary>
    public TrunkStabilityLevel TrunkStability { get; set; }

    /// <summary>Indicates whether the patient is at risk for pressure sores (determines the type of seat cushion).</summary>
    public bool HasPressureSoresRisk { get; set; }
}