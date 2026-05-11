using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

/// <summary>
/// Repository for Specialist entity.
/// Provides specialist-specific queries in addition to generic CRUD.
/// </summary>
public class SpecialistRepository : GenericRepository<Specialist>, ISpecialistRepository
{
    public SpecialistRepository(SQLiteAsyncConnection db) : base(db) { }

    /// <summary>
    /// Returns a specialist by their email, or null if not found.
    /// </summary>
    public async Task<Specialist?> GetByEmailAsync(string email)
        => await _db.Table<Specialist>()
              .Where(s => s.Email == email)
              .FirstOrDefaultAsync();

    public async Task<List<Specialist>> GetAllActiveAsync()
        => await _db.Table<Specialist>()
                    .Where(s => s.IsActive)
                    .ToListAsync();

    public Task<List<Specialist>> GetByClinicAsync(string clinic)
        => _db.Table<Specialist>()
              .Where(s => s.Clinic == clinic)
              .ToListAsync();
}