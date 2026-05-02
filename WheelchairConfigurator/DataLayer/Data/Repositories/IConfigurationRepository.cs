using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository interface for the <see cref="Configuration"/> entity.
/// Extends the generic CRUD contract with configuration-specific queries.
/// </summary>
public interface IConfigurationRepository : IRepository<Configuration>
{
    /// <summary>
    /// Returns all configurations created by the specified specialist.
    /// </summary>
    /// <param name="specialistId">The ID of the specialist whose configurations to retrieve.</param>
    /// <returns>A list of <see cref="Configuration"/> entities belonging to the specialist.</returns>
    Task<List<Configuration>> GetBySpecialistIdAsync(int specialistId);
}
