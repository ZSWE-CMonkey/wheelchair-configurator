using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository for Model3D entity.
/// Provides 3D model-specific queries in addition to generic CRUD.
/// </summary>
public class Model3DRepository : GenericRepository<Model3D>
{
    public Model3DRepository(SQLiteAsyncConnection db) : base(db) { }

    /// <summary>
    /// Returns the 3D model for the specified component, or null if not found.
    /// </summary>
    public async Task<Model3D?> GetByComponentIdAsync(int componentId)
        => await _db.Table<Model3D>()
                    .Where(m => m.ComponentId == componentId)
                    .FirstOrDefaultAsync();
}