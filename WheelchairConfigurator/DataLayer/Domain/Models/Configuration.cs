using System;
using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("Configuration")]
public class Configuration
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int SpecialistId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string PatientIdentificator { get; set; } = string.Empty;
}