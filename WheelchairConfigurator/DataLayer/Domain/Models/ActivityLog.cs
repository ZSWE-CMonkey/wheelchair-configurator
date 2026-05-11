using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("ActivityLog")]
public class ActivityLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.Now;

    public int? SpecialistId { get; set; }
    public string SpecialistName { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Detail { get; set; }
}
