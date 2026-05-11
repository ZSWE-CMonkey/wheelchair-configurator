using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

public interface IActivityLogRepository : IRepository<ActivityLog>
{
    Task<List<ActivityLog>> GetRecentAsync(int pageSize = 100);
}
