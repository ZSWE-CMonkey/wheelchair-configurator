using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(SQLiteAsyncConnection db) : base(db) { }

    public Task<Patient?> GetByIdentificatorAsync(string patientIdentificator, int specialistId)
        => _db.Table<Patient>()
              .Where(p => p.PatientIdentificator == patientIdentificator && p.SpecialistId == specialistId)
              .FirstOrDefaultAsync();
}
