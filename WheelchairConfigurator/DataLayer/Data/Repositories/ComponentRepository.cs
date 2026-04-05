using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository for Component entity.
/// Provides component-specific queries in addition to generic CRUD.
/// </summary>
public class ComponentRepository : GenericRepository<Component>
{
    public ComponentRepository(SQLiteAsyncConnection db) : base(db) { }

    /// <summary>
    /// Returns all components belonging to the specified category.
    /// </summary>
    public Task<List<Component>> GetByCategoryIdAsync(int categoryId)
        => _db.Table<Component>()
              .Where(c => c.CategoryId == categoryId)
              .ToListAsync();

    /// <summary>
    /// Returns a component by its name, or null if not found.
    /// </summary>
    public async Task<Component?> GetByNameAsync(string name)
        => await _db.Table<Component>()
              .Where(c => c.Name == name)
              .FirstOrDefaultAsync();
    
    /// <summary>
        /// Returns all components matching the given IDs in a single query.
        /// </summary>
        public Task<List<Component>> GetByIdsAsync(List<int> ids)
            => _db.Table<Component>()
                  .Where(c => ids.Contains(c.Id))
                  .ToListAsync();
}