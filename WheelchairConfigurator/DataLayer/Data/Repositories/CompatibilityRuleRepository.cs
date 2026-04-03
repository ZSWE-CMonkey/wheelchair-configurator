using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository for CompatibilityRule entity.
/// Provides compatibility-specific queries in addition to generic CRUD.
/// </summary>
public class CompatibilityRuleRepository : GenericRepository<CompatibilityRule>
{
    public CompatibilityRuleRepository(SQLiteAsyncConnection db) : base(db) { }

    /// <summary>
    /// Returns all compatibility rules involving the specified component.
    /// </summary>
    public Task<List<CompatibilityRule>> GetRulesForComponentAsync(int componentId)
        => _db.Table<CompatibilityRule>()
              .Where(r => r.ComponentAId == componentId || r.ComponentBId == componentId)
              .ToListAsync();

    /// <summary>
    /// Returns the compatibility rule between two components, or null if not defined.
    /// </summary>
    public async Task<CompatibilityRule?> GetRuleAsync(int componentAId, int componentBId)
        => await _db.Table<CompatibilityRule>()
              .Where(r => (r.ComponentAId == componentAId && r.ComponentBId == componentBId) ||
                          (r.ComponentAId == componentBId && r.ComponentBId == componentAId))
              .FirstOrDefaultAsync();

    /// <summary>
    /// Returns true if the two components are compatible, false if incompatible,
    /// or null if no rule is defined between them.
    /// </summary>
    public async Task<bool?> AreCompatibleAsync(int componentAId, int componentBId)
    {
        var rule = await GetRuleAsync(componentAId, componentBId);
        return rule?.IsCompatible;
    }
}