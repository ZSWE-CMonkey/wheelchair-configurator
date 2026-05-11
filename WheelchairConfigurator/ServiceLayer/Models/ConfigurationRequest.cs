namespace WheelchairConfigurator.ServiceLayer.Models;

public class ConfigurationRequest
{
    public int SpecialistId { get; set; }
    public string SpecialistName { get; set; } = string.Empty;

    public PatientProfileModel? Patient { get; set; }

    public List<int> SelectedComponentIds { get; set; } = new();

    public int PatientMeasurementId { get; set; }
    public string PatientBirthNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
}
