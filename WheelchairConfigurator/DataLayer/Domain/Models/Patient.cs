using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("Patient")]
public class Patient
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed(Unique = true)]
    public string BirthNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int CreatedBySpecialistId { get; set; }
    public string CreatedBySpecialistName { get; set; } = string.Empty;
}
