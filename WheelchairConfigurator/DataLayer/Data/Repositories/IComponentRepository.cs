using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository interface for the <see cref="Component"/> entity.
/// Extends the generic CRUD contract with component-specific queries.
/// </summary>
public interface IComponentRepository : IRepository<Component>
{
    /// <summary>
    /// Returns all components belonging to the specified category.
    /// </summary>
    /// <param name="categoryId">The ID of the category to filter by.</param>
    /// <returns>A list of <see cref="Component"/> entities in the given category.</returns>
    Task<List<Component>> GetByCategoryIdAsync(int categoryId);

    /// <summary>
    /// Returns all components matching the provided list of IDs in a single query.
    /// </summary>
    /// <param name="ids">The list of component IDs to retrieve.</param>
    /// <returns>A list of matching <see cref="Component"/> entities.</returns>
    Task<List<Component>> GetByIdsAsync(List<int> ids);
}
