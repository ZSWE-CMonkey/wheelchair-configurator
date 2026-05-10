using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

public class PatientMeasurementRepository : GenericRepository<PatientMeasurement>, IPatientMeasurementRepository
{
    public PatientMeasurementRepository(SQLiteAsyncConnection db) : base(db) { }

    public async Task<List<PatientMeasurement>> GetByPatientIdAsync(int patientId)
        => await _db.Table<PatientMeasurement>()
                    .Where(m => m.PatientId == patientId)
                    .OrderByDescending(m => m.MeasuredAt)
                    .ToListAsync();

    public Task<PatientMeasurement?> GetLatestForPatientAsync(int patientId)
        => _db.Table<PatientMeasurement>()
              .Where(m => m.PatientId == patientId)
              .OrderByDescending(m => m.MeasuredAt)
              .FirstOrDefaultAsync();
}
