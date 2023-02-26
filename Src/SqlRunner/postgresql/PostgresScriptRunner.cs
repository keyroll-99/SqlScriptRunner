using Npgsql;
using SqlRunner.Abstraction;
using SqlRunner.models;
using SqlRunner.valueObjects;

namespace SqlRunner.postgresql;

internal class PostgresScriptRunner : IDatabaseScriptRunner
{
    private readonly NpgsqlConnection _connection;
    private readonly string _deployScriptsTableName;


    public PostgresScriptRunner(string connectionString, string deployScriptsTableName)
    {
        _connection = new NpgsqlConnection(connectionString);
        _deployScriptsTableName = deployScriptsTableName;

    }

    public async Task InitConnectionAsync()
    {
        await _connection.OpenAsync();
    }

    public async Task CloseConnectionAsync()
    {
        await _connection.CloseAsync();
    }

    public async Task<bool> IsDeployScriptTableExistsAsync()
    {
        await using var cmd = new NpgsqlCommand(
            $"SELECT exists( SELECT * FROM information_schema.tables t WHERE t.table_name = '{_deployScriptsTableName}')",
            _connection);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        var result = reader.GetBoolean(0);

        return result;
    }

    public async Task CreateDeployScriptTable()
    {
        await using var cmd = new NpgsqlCommand(
            $"CREATE TABLE \"{_deployScriptsTableName}\"("
            + "Id serial primary key,"
            + "Path text not null,"
            + "Name text not null,"
            + "CreateDate date default NOW()"
            + ");"
            , _connection);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SaveLogAboutScriptRun(Query query)
    {
        await using var command =
            new NpgsqlCommand(
                $"INSERT INTO \"{_deployScriptsTableName}\" (Path, Name) VALUES (@scriptPath, @scriptName)",
                _connection)
            {
                Parameters =
                {
                    new NpgsqlParameter("scriptPath", query.FilePath.GetFileDictionaryPath().Value),
                    new NpgsqlParameter("scriptName", query.FileName.Value)
                }
            };
        await command.ExecuteNonQueryAsync();
    }

    public async Task RunScriptAsync(Query query)
    {
        await using var command = new NpgsqlCommand(query.QueryContent, _connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<DeployScript>> GetExecutedFile(DictionaryPath dictionaryPath)
    {
        var result = new List<DeployScript>();
        var query =
            $"SELECT name, path FROM \"{_deployScriptsTableName}\" WHERE path = @scriptPath";

        await using var command = new NpgsqlCommand(query, _connection)
        {
            Parameters = { new NpgsqlParameter("scriptPath", dictionaryPath.Value) }
        };

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new DeployScript
            {
                Name = reader.GetString(0),
                Path = reader.GetString(1)
            });
        }

        return result;
    }
}