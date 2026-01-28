using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Comercial.Data;

public interface IGenericRepository
{
    Task<IEnumerable<T>> GetAllAsync<T>(IDbConnection conn, object? filters = null);
    Task<T?> GetByIdAsync<T>(object id, IDbConnection conn);
    Task<long> InsertAsync<T>(T entity, IDbConnection conn);
    Task<int> UpdateAsync<T>(T entity, IDbConnection conn);
    Task<int> DeleteAsync<T>(object id, IDbConnection conn, bool soft = true);
}