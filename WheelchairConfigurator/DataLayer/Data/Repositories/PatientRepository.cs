using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(SQLiteAsyncConnection db) : base(db) { }

    public Task<Patient?> GetByBirthNumberAsync(string birthNumber)
        => _db.Table<Patient>()
              .Where(p => p.BirthNumber == birthNumber)
              .FirstOrDefaultAsync();

    public async Task<List<Patient>> GetAllActiveAsync()
        => await _db.Table<Patient>()
                    .Where(p => p.IsActive)
                    .ToListAsync();
}
