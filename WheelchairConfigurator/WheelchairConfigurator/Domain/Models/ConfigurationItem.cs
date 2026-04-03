using SQLite;

namespace WheelchairConfigurator.Domain.Models;

[Table("ConfigurationItem")]
public class ConfigurationItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed(Name = "uq_config_item", Order = 1, Unique = true)]
    public int ConfigurationId { get; set; }

    [Indexed(Name = "uq_config_item", Order = 2, Unique = true)]
    public int ComponentId { get; set; }

    public int Quantity { get; set; } = 1;
}