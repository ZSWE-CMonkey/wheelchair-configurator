namespace WheelchairConfigurator.ServiceLayer.Models;

public class PatientModel
{
    public int Id { get; set; }
    public string BirthNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int CreatedBySpecialistId { get; set; }
    public string CreatedBySpecialistName { get; set; } = string.Empty;

    public string FullName => $"{LastName} {FirstName}";
}
