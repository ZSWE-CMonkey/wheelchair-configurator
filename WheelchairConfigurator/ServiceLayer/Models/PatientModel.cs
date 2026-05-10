namespace WheelchairConfigurator.ServiceLayer.Models;

public class PatientModel
{
    public string PatientIdentificator { get; set; } = string.Empty;
    public int SpecialistId { get; set; }
    public double BodyHeight { get; set; }
    public double PelvisWidth { get; set; }
    public double ThighLength { get; set; }
    public double Weight { get; set; }
    public string BodyStability { get; set; } = string.Empty;
    public bool HeadStability { get; set; } = true;
    public string BedsoreRisk { get; set; } = string.Empty;
    public string Control { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public bool Legs { get; set; } = true;
    public string Pain { get; set; } = string.Empty;
}
