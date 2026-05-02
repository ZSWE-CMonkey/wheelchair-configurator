using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository interface for the <see cref="ConfigurationItem"/> entity.
/// Extends the generic CRUD contract with configuration item-specific queries.
/// </summary>
public interface IConfigurationItemRepository : IRepository<ConfigurationItem>
{
    /// <summary>
    /// Returns all items belonging to the specified configuration.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration whose items to retrieve.</param>
    /// <returns>A list of <see cref="ConfigurationItem"/> entities for the given configuration.</returns>
    Task<List<ConfigurationItem>> GetByConfigurationIdAsync(int configurationId);
}
