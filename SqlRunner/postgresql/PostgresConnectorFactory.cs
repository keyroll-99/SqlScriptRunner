using Npgsql;

namespace SqlRunner.postgresql;

public class PostgresConnectorFactory
{
    public NpgsqlConnection CreateNpqsqlConnection(string connectionString)
    {
        return  new NpgsqlConnection(connectionString);
    }
}