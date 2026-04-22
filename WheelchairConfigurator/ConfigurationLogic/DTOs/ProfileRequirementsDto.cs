namespace ConfigurationLogic.DTOs;

// Derived profile needs
public class ProfileRequirementsDto
{
    public int? MinimumSeatWidthCm { get; set; }
    public int? MaximumSeatWidthCm { get; set; }
    public int? MinimumSeatDepthCm { get; set; }
    public int? MaximumSeatDepthCm { get; set; }
    public int MinimumWeightCapacityKg { get; set; }
    public int MinimumDrivePowerLevel { get; set; }
    public int MinimumPressureReliefLevel { get; set; }
    public int MinimumComfortLevel { get; set; }
    public string BackrestHeightRecommendation { get; set; } = string.Empty;
    public string ChassisRecommendation { get; set; } = string.Empty;
    public bool NeedsHeadrest { get; set; }
    public bool NeedsTilt { get; set; }
    public bool NeedsLateralSupports { get; set; }
    public bool NeedsAlternativeControl { get; set; }
    public bool NeedsPressureRelief { get; set; }
    public bool NeedsLegSupportAdaptation { get; set; }
}

