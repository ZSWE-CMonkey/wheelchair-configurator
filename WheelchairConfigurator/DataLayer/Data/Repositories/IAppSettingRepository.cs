using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

public interface IAppSettingRepository
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value);
}
