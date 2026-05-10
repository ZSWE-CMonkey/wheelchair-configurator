namespace WheelchairConfigurator.ServiceLayer.Models;

public class SpecialistModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Clinic { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
