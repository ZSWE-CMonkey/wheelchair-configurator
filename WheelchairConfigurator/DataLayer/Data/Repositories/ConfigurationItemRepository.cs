using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository for ConfigurationItem entity.
/// Provides configuration item-specific queries in addition to generic CRUD.
/// </summary>
public class ConfigurationItemRepository : GenericRepository<ConfigurationItem>, IConfigurationItemRepository
{
    public ConfigurationItemRepository(SQLiteAsyncConnection db) : base(db) { }

    /// <summary>
    /// Returns all items belonging to the specified configuration.
    /// </summary>
    public Task<List<ConfigurationItem>> GetByConfigurationIdAsync(int configurationId)
        => _db.Table<ConfigurationItem>()
              .Where(i => i.ConfigurationId == configurationId)
              .ToListAsync();
}