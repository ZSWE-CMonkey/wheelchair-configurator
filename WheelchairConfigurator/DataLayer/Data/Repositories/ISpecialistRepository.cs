using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository interface for the <see cref="Specialist"/> entity.
/// Inherits the full generic CRUD contract from <see cref="IRepository{T}"/>.
/// No specialist-specific queries are required at this time.
/// </summary>
public interface ISpecialistRepository : IRepository<Specialist>
{
}
