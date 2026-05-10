using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByIdentificatorAsync(string patientIdentificator, int specialistId);
}
