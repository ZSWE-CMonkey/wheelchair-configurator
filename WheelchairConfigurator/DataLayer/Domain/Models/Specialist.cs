using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("Specialist")]
public class Specialist
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string FirstName { get; set; } = string.Empty;

    [NotNull]
    public string LastName { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? Clinic { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}