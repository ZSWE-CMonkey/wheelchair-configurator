using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

public interface ISpecialistRepository : IRepository<Specialist>
{
    Task<List<Specialist>> GetAllActiveAsync();
}
