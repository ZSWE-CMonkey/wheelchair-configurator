using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("ComponentSpecs")]
public class ComponentSpecs
{
    [PrimaryKey]
    public int ComponentId { get; set; }

    public int WeightCapacityKg { get; set; }
    public int SeatWidthCm { get; set; }
    public int MaxSpeedKmh { get; set; }
}