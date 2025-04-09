using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class GenericRepository<T>
{
    private readonly DbConnectionFactory _dbFactory;

    public GenericRepository(DbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<T>> GetAllAsync(string query)
    {
        using var db = _dbFactory.CreateConnection();
        return await db.QueryAsync<T>(query);
    }

    public async Task<int> ExecuteAsync(string query, object parameters = null)
    {
        using var db = _dbFactory.CreateConnection();
        return await db.ExecuteAsync(query, parameters);
    }
}