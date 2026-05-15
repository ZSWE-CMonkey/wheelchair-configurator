namespace WheelchairConfigurator.Data.DTOs;

public class PatientMeasurementDto
{
    public string PatientBirthNumber { get; set; } = string.Empty;
    public string CreatedBySpecialistFullName { get; set; } = string.Empty;

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
