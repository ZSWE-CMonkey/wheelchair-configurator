using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

public interface IPatientMeasurementRepository : IRepository<PatientMeasurement>
{
    Task<List<PatientMeasurement>> GetByPatientIdAsync(int patientId);
    Task<PatientMeasurement?> GetLatestForPatientAsync(int patientId);
}
