using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("Patient")]
public class Patient
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int SpecialistId { get; set; }

    [Indexed]
    public string PatientIdentificator { get; set; } = string.Empty;

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
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
