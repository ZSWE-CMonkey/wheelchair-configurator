using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository interface for the <see cref="Category"/> entity.
/// Extends the generic CRUD contract with category-specific queries.
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Returns a category by its name, or null if not found.
    /// </summary>
    /// <param name="name">The exact name of the category to search for.</param>
    /// <returns>The matching <see cref="Category"/>, or null.</returns>
    Task<Category?> GetByNameAsync(string name);

    /// <summary>
    /// Returns all categories matching the provided list of IDs in a single query.
    /// </summary>
    /// <param name="ids">The list of category IDs to retrieve.</param>
    /// <returns>A list of matching <see cref="Category"/> entities.</returns>
    Task<List<Category>> GetByIdsAsync(List<int> ids);
}
