namespace ConfigurationLogic.DTOs;

// Component specs payload
public class ComponentSpecsOutputDto
{
    public int ComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public int? WeightCapacityKg { get; set; }
    public int? SeatWidthCm { get; set; }
    public int? SeatDepthCm { get; set; }
    public int? BackrestHeightLevel { get; set; }
    public int? MaxSpeedKmh { get; set; }
    public int? DrivePowerLevel { get; set; }
    public bool? SupportsTilt { get; set; }
    public bool? SupportsRecline { get; set; }
    public bool? SupportsLateralSupport { get; set; }
    public bool? HasHeadSupport { get; set; }
    public int? PressureReliefLevel { get; set; }
    public string? ControlMode { get; set; }
    public string? EnvironmentType { get; set; }
    public bool? SupportsLegRestAdjustment { get; set; }
    public int? ComfortLevel { get; set; }
}

