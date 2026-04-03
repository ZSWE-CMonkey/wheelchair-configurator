namespace WheelchairConfigurator.Data.DTOs;

public class ComponentSpecsDto
{
    public string ComponentName { get; set; } = string.Empty; // Vazba na konkrétní díl
    public int WeightCapacityKg { get; set; }
    public int SeatWidthCm { get; set; }
    public int MaxSpeedKmh { get; set; }
}