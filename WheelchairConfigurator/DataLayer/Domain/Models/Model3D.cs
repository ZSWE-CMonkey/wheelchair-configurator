using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("Model3D")]
public class Model3D
{
    [PrimaryKey]
    public int ComponentId { get; set; }

    public string? FilePath { get; set; }
    public string? TextureId { get; set; }

    public decimal AnchorX { get; set; } = 0.0m;
    public decimal AnchorY { get; set; } = 0.0m;
    public decimal AnchorZ { get; set; } = 0.0m;
}