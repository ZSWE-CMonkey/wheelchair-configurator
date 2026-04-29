using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("Category")]
public class Category
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    [NotNull]
    public string RoleKey { get; set; } = "unknown";
}