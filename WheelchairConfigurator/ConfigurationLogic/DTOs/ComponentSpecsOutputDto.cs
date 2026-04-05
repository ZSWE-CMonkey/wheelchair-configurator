namespace ConfigurationLogic.DTOs;

// Component specs payload
public class ComponentSpecsOutputDto
{
    public int ComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public int WeightCapacityKg { get; set; }
    public int SeatWidthCm { get; set; }
    public int MaxSpeedKmh { get; set; }
}

