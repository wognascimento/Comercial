using Comercial.DataBase;
using Npgsql;
using System.Data;

namespace Comercial.Data;

public static class DbConnectionFactory
{
    private static readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;

    public static IDbConnection Create()
        => new NpgsqlConnection(BaseSettings.ConnectionString);
}
