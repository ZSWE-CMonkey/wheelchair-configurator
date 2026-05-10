namespace WheelchairConfigurator.ServiceLayer.Models;

public class ConfigurationModel
{
    public int Id { get; set; }
    public int SpecialistId { get; set; }
    public string SpecialistName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int PatientMeasurementId { get; set; }
    public string PatientBirthNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
}
