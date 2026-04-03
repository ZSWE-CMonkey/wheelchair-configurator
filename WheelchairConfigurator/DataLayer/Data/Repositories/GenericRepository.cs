using SQLite;

namespace WheelchairConfigurator.Data.Repositories;

public class GenericRepository<T> : IRepository<T> where T : new()
{
    protected readonly SQLiteAsyncConnection _db;

    public GenericRepository(SQLiteAsyncConnection db)
    {
        _db = db;
    }

    public Task<List<T>> GetAllAsync()
        => _db.Table<T>().ToListAsync();

    public async Task<T?> GetByIdAsync(int id)
       => await _db.FindAsync<T>(id);

    public Task<int> InsertAsync(T entity)
        => _db.InsertAsync(entity);

    public Task<int> UpdateAsync(T entity)
        => _db.UpdateAsync(entity);

    public Task<int> DeleteAsync(T entity)
        => _db.DeleteAsync(entity);
}