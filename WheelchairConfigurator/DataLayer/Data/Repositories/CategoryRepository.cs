using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository for Category entity.
/// Provides category-specific queries in addition to generic CRUD.
/// </summary>
public class CategoryRepository : GenericRepository<Category>
{
    public CategoryRepository(SQLiteAsyncConnection db) : base(db) { }

    /// <summary>
    /// Returns a category by its name, or null if not found.
    /// </summary>
    public async Task<Category?> GetByNameAsync(string name)
        => await _db.Table<Category>()
              .Where(c => c.Name == name)
              .FirstOrDefaultAsync();
}