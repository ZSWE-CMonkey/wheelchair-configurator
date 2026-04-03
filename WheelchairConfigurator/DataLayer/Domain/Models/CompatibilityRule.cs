using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("CompatibilityRule")]
public class CompatibilityRule
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed(Name = "uq_compatibility_pair", Order = 1, Unique = true)]
    public int ComponentAId { get; set; }

    [Indexed(Name = "uq_compatibility_pair", Order = 2, Unique = true)]
    public int ComponentBId { get; set; }

    public bool IsCompatible { get; set; } = true;
}