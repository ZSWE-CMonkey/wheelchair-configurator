namespace WheelchairConfigurator.ServiceLayer.Models;

public class Model3DModel
{
    public int ComponentId { get; set; }
    public string? FilePath { get; set; }
    public string? TextureId { get; set; }
    public float Scale { get; set; } = 1.0f;

    public float AnchorX { get; set; } = 0.0f;
    public float AnchorY { get; set; } = 0.0f;
    public float AnchorZ { get; set; } = 0.0f;

    public float RotationX { get; set; } = 0.0f;
    public float RotationY { get; set; } = 0.0f;
    public float RotationZ { get; set; } = 0.0f;
}
