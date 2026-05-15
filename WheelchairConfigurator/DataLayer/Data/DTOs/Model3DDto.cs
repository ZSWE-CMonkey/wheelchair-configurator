namespace WheelchairConfigurator.Data.DTOs;

public class Model3DDto
{
    public string ComponentName { get; set; } = string.Empty; // Vazba na konkrétní díl
    public string? FilePath { get; set; }
    public string? TextureId { get; set; }
    public decimal AnchorX { get; set; } = 0.0m;
    public decimal AnchorY { get; set; } = 0.0m;
    public decimal AnchorZ { get; set; } = 0.0m;

    public decimal Scale { get; set; } = 1.0m;

    public decimal RotationX { get; set; } = 0.0m;
    public decimal RotationY { get; set; } = 0.0m;
    public decimal RotationZ { get; set; } = 0.0m;
}