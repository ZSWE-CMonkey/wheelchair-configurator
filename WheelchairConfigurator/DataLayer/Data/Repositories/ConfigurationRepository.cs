using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository for Configuration entity.
/// Provides configuration-specific queries in addition to generic CRUD.
/// </summary>
public class ConfigurationRepository : GenericRepository<Configuration>, IConfigurationRepository
{
    public ConfigurationRepository(SQLiteAsyncConnection db) : base(db) { }

    /// <summary>
    /// Returns all configurations created by the specified specialist.
    /// </summary>
    public Task<List<Configuration>> GetBySpecialistIdAsync(int specialistId)
        => _db.Table<Configuration>()
              .Where(c => c.SpecialistId == specialistId)
              .ToListAsync();
}