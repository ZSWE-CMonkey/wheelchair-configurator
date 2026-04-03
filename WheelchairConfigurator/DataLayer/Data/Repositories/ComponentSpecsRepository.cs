using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository for ComponentSpecs entity.
/// Provides specs-specific queries in addition to generic CRUD.
/// </summary>
public class ComponentSpecsRepository : GenericRepository<ComponentSpecs>
{
    public ComponentSpecsRepository(SQLiteAsyncConnection db) : base(db) { }

    /// <summary>
    /// Returns the specs for the specified component, or null if not found.
    /// </summary>
    public async Task<ComponentSpecs?> GetByComponentIdAsync(int componentId)
        => await _db.Table<ComponentSpecs>()
              .Where(s => s.ComponentId == componentId)
              .FirstOrDefaultAsync();
}