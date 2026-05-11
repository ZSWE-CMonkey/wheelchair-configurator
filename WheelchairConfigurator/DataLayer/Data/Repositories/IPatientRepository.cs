using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByBirthNumberAsync(string birthNumber);
    Task<List<Patient>> GetAllActiveAsync();
}
