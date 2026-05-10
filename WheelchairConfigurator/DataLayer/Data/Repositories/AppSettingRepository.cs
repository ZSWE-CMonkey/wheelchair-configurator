using SQLite;
using WheelchairConfigurator.Domain.Models;

namespace WheelchairConfigurator.Data.Repositories;

public class AppSettingRepository : IAppSettingRepository
{
    private readonly SQLiteAsyncConnection _db;

    public AppSettingRepository(SQLiteAsyncConnection db) => _db = db;

    public async Task<string?> GetAsync(string key)
    {
        var row = await _db.Table<AppSetting>().Where(s => s.Key == key).FirstOrDefaultAsync();
        return row?.Value;
    }

    public async Task SetAsync(string key, string value)
    {
        var existing = await _db.Table<AppSetting>().Where(s => s.Key == key).FirstOrDefaultAsync();
        if (existing is null)
            await _db.InsertAsync(new AppSetting { Key = key, Value = value });
        else
        {
            existing.Value = value;
            await _db.UpdateAsync(existing);
        }
    }
}
