// IRepository.cs
public interface IRepository<T> where T : new()
{
    /// <summary>Returns all records of type T.</summary>
    Task<List<T>> GetAllAsync();

    /// <summary>Returns a single record by ID, or null if not found.</summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>Inserts a new record. Returns number of affected rows.</summary>
    Task<int> InsertAsync(T entity);

    /// <summary>Updates an existing record. Returns number of affected rows.</summary>
    Task<int> UpdateAsync(T entity);

    /// <summary>Deletes a record. Returns number of affected rows.</summary>
    Task<int> DeleteAsync(T entity);
}