using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("Component")]
public class Component
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int CategoryId { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    public string? CatalogUrl { get; set; }

    public decimal Price { get; set; }
}