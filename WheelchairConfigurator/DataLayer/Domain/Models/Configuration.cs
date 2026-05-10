using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("Configuration")]
public class Configuration
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int SpecialistId { get; set; }

    public string SpecialistName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int PatientMeasurementId { get; set; }
    public string PatientBirthNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;

    public string Hash { get; set; } = string.Empty;
}