using System.Data;
using MySqlConnector;
using SqlRunner.models;
using SqlRunner.valueObjects;

namespace SqlRunner.mysql;

internal class MysqlScriptRunner : ScriptRunner
{
    private readonly MySqlConnection _connection;

    public MysqlScriptRunner(SetupModel setupModel) : base(setupModel)
    {
        _connection = new MySqlConnection(setupModel.ConnectionString);
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
        const string sql = "SELECT COUNT(*)" +
                           "FROM information_schema.tables " +
                           " WHERE table_schema = DATABASE() " +
                           "AND table_name LIKE @table";

        await using var command = new MySqlCommand(sql, _connection)
        {
            Parameters =
            {
                new MySqlParameter("table", SetupModel.DeployScriptsTableName)
                {
                    DbType = DbType.String
                }
            }
        };

        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var result = reader.GetInt32(0) > 0;

        return result;
    }

    protected override async Task CreateDeployScriptTable()
    {
        var sql = $"CREATE TABLE {SetupModel.DeployScriptsTableName}(" +
                  "Id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY," +
                  "Path TEXT NOT NULL," +
                  "Name TEXT NOT NULL," +
                  "CreateDate DATETIME default NOW()" +
                  ")";

        await using var cmd = new MySqlCommand(sql, _connection);
        await cmd.ExecuteNonQueryAsync();
    }

    protected override async Task SaveLogAboutScriptRun(Query query)
    {

        await using var command =
            new MySqlCommand(
                $"INSERT INTO {SetupModel.DeployScriptsTableName} (Path, Name) VALUES (@scriptPath, @scriptName)",
                _connection
            )
            {
                Parameters =
                {
                    new MySqlParameter("scriptPath", query.FilePatch)
                    {
                        DbType = DbType.String
                    },
                    new MySqlParameter("scriptName", query.FileName)
                    {
                        DbType = DbType.String
                    }
                }
            };

        await command.ExecuteNonQueryAsync();
    }

    protected override async Task RunScriptAsync(Query query)
    {
        await using var command = new MySqlCommand(query.QueryContent, _connection);
        await command.ExecuteNonQueryAsync();
    }

    protected override async Task<List<DeployScript>> GetExecutedFile(FilePatch filePatch)
    {
        var result = new List<DeployScript>();
        var query =
            $"SELECT name, path FROM {SetupModel.DeployScriptsTableName} WHERE path LIKE @scriptPath";

        await using var command = new MySqlCommand(query, _connection)
        {
            Parameters = { new MySqlParameter("scriptPath", filePatch) }
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