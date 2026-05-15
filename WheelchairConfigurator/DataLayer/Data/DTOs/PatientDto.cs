namespace WheelchairConfigurator.Data.DTOs;

public class PatientDto
{
    public string BirthNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CreatedBySpecialistFullName { get; set; } = string.Empty;
}
