using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Configuration;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MariaDb");
    }

    public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
}