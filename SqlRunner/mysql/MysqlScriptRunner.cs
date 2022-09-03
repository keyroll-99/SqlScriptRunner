using System.Data;
using MySqlConnector;
using SqlRunner.models;

namespace SqlRunner.mysql;

public class MysqlScriptRunner : ScriptRunner
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
                  "Name TExt NOT NULL," +
                  "CreateDate DATETIME default NOW()" +
                  ")";
        
        await using var cmd = new MySqlCommand(sql, _connection);
        await cmd.ExecuteNonQueryAsync();
    }

    protected override async Task SaveLogAboutScriptRun(string filePath)
    {
        var path = GetPath(filePath);
        var name = GetFileName(filePath);

        await using var command =
            new MySqlCommand(
                $"INSERT INTO {SetupModel.DeployScriptsTableName} (Path, Name) VALUES (@scriptPath, @scriptName)",
                _connection
            )
            {
                Parameters =
                {
                    new MySqlParameter("scriptPath", path)
                    {
                        DbType = DbType.String
                    },
                    new MySqlParameter("scriptName", name)
                    {
                        DbType = DbType.String
                    }
                }
            };

        await command.ExecuteNonQueryAsync();
    }

    protected override async Task RunScriptAsync(string filePath)
    {
        var query = GetFileContent(filePath);
        await using var command = new MySqlCommand(query, _connection);
        await command.ExecuteNonQueryAsync();
    }

    protected override async Task<List<DeployScript>> GetExecutedFile(string path)
    {
        var result = new List<DeployScript>();
        var query =
            $"SELECT name, path FROM {SetupModel.DeployScriptsTableName} WHERE path LIKE @scriptPath";

        await using var command = new MySqlCommand(query, _connection)
        {
            Parameters = { new MySqlParameter("scriptPath", path) }
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