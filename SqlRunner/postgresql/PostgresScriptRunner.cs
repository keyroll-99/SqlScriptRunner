using Npgsql;
using SqlRunner.models;

namespace SqlRunner.postgresql;

public class PostgresScriptRunner : ScriptRunner
{
    private readonly NpgsqlConnection _connection;

    public PostgresScriptRunner(SetupModel setupModel) : base(setupModel)
    {
        _connection = new NpgsqlConnection(setupModel.ConnectionString);

    }

    protected override async Task InitConnectionAsync()
    {
        await _connection.OpenAsync();
    }

    protected override async Task CloseConnectionAsync()
    {
        await _connection.CloseAsync();
    }

    protected override async Task<bool> IsDeployScriptTableExistsAsync()
    {
        await using var cmd = new NpgsqlCommand(
            $"SELECT exists( SELECT * FROM information_schema.tables t WHERE t.table_name = '{SetupModel.DeployScriptsTableName}')",
            _connection);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        var result = reader.GetBoolean(0);

        return result;
    }

    protected override async Task CreateDeployScriptTable()
    {
        await using var cmd = new NpgsqlCommand(
            $"CREATE TABLE \"{SetupModel.DeployScriptsTableName}\"("
            + "Id serial primary key,"
            + "Path text not null,"
            + "Name text not null,"
            + "CreateDate date default NOW()"
            + ");"
            , _connection);

        await cmd.ExecuteNonQueryAsync();
    }

    protected override async Task SaveLogAboutScriptRun(string filePath)
    {
        var path = GetPath(filePath);
        var name = GetFileName(filePath);

        await using var command =
            new NpgsqlCommand(
                $"INSERT INTO \"{SetupModel.DeployScriptsTableName}\" (Path, Name) VALUES (@scriptPath, @scriptName)",
                _connection)
            {
                Parameters =
                {
                    new NpgsqlParameter("scriptPath", path),
                    new NpgsqlParameter("scriptName", name)
                }
            };
        await command.ExecuteNonQueryAsync();
    }

    protected override async Task RunScriptAsync(string filePath)
    {
        var query = GetFileContent(filePath);
        await using var command = new NpgsqlCommand(query, _connection);
        await command.ExecuteNonQueryAsync();
    }

    protected override async Task<List<DeployScript>> GetExecutedFile(string path)
    {
        var result = new List<DeployScript>();
        var query =
            $"SELECT name, path FROM \"{SetupModel.DeployScriptsTableName}\" WHERE path = @scriptPath";

        await using var command = new NpgsqlCommand(query, _connection)
        {
            Parameters = { new NpgsqlParameter("scriptPath", path) }
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