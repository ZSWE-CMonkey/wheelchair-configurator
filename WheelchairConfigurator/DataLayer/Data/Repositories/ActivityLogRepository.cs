using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

public class ActivityLogRepository : GenericRepository<ActivityLog>, IActivityLogRepository
{
    public ActivityLogRepository(SQLiteAsyncConnection db) : base(db) { }

    public async Task<List<ActivityLog>> GetRecentAsync(int pageSize = 100)
        => await _db.Table<ActivityLog>()
                    .OrderByDescending(l => l.OccurredAt)
                    .Take(pageSize)
                    .ToListAsync();
}
