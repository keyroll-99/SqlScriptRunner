using System.Data;
using MySqlConnector;
using SqlRunner.Abstraction;
using SqlRunner.models;
using SqlRunner.valueObjects;

namespace SqlRunner.mysql;

internal class MysqlScriptRunner : IDatabaseScriptRunner
{
    private readonly MySqlConnection _connection;
    private readonly string _deployScriptsTableName;

    public MysqlScriptRunner(string connectionString, string deployScriptsTableName)
    {
        _connection = new MySqlConnection(connectionString);
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
        const string sql = "SELECT COUNT(*)" +
                           "FROM information_schema.tables " +
                           " WHERE table_schema = DATABASE() " +
                           "AND table_name LIKE @table";

        await using var command = new MySqlCommand(sql, _connection)
        {
            Parameters =
            {
                new MySqlParameter("table", _deployScriptsTableName)
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

    public async Task CreateDeployScriptTable()
    {
        var sql = $"CREATE TABLE {_deployScriptsTableName}(" +
                  "Id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY," +
                  "Path TEXT NOT NULL," +
                  "Name TEXT NOT NULL," +
                  "CreateDate DATETIME default NOW()" +
                  ")";

        await using var cmd = new MySqlCommand(sql, _connection);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SaveLogAboutScriptRun(Query query)
    {

        await using var command =
            new MySqlCommand(
                $"INSERT INTO {_deployScriptsTableName} (Path, Name) VALUES (@scriptPath, @scriptName)",
                _connection
            )
            {
                Parameters =
                {
                    new MySqlParameter("scriptPath", query.FilePath.GetFileDictionaryPath().Value)
                    {
                        DbType = DbType.String
                    },
                    new MySqlParameter("scriptName", query.FileName.Value)
                    {
                        DbType = DbType.String
                    }
                }
            };

        await command.ExecuteNonQueryAsync();
    }

    public async Task RunScriptAsync(Query query)
    {
        await using var command = new MySqlCommand(query.QueryContent, _connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<DeployScript>> GetExecutedFile(DictionaryPath dictionaryPath)
    {
        var result = new List<DeployScript>();
        var query =
            $"SELECT name, path FROM {_deployScriptsTableName} WHERE path LIKE @scriptPath";

        await using var command = new MySqlCommand(query, _connection)
        {
            Parameters = { new MySqlParameter("scriptPath", dictionaryPath.Value) }
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