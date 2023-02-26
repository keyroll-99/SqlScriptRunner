using System.Data;
using Microsoft.Data.SqlClient;
using SqlRunner.Abstraction;
using SqlRunner.models;
using SqlRunner.valueObjects;

namespace SqlRunner.mssql;

internal class MssqlScriptRunner : IDatabaseScriptRunner
{
    private readonly SqlConnection _connection;
    private readonly string _deployScriptsTableName;

    public MssqlScriptRunner(string connectionString, string deployScriptsTableName)
    {
        _connection = new SqlConnection(connectionString);
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
        const string sql = "Select 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @table;";

        await using var command = new SqlCommand(sql, _connection)
        {
            Parameters =
            {
                new SqlParameter("table", _deployScriptsTableName)
                {
                    DbType = DbType.String
                }
            }
        };
        await using var reader = await command.ExecuteReaderAsync();

        var result = await reader.ReadAsync();

        return result;
    }

    public async Task CreateDeployScriptTable()
    {
        var sql = $"CREATE TABLE {_deployScriptsTableName}(" +
                  "Id INT IDENTITY(1,1) PRIMARY KEY," +
                  "Path TEXT NOT NULL," +
                  "Name TEXT NOT NULL," +
                  "CreateDate DATETIME default getdate()" +
                  ")";

        await using var command = new SqlCommand(sql, _connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task SaveLogAboutScriptRun(Query query)
    {
        await using var command =
            new SqlCommand(
                $"INSERT INTO {_deployScriptsTableName} (Path, Name) VALUES (@scriptPath, @scriptName)",
                _connection
            )
            {
                Parameters =
                {
                    new SqlParameter("scriptPath", query.FilePath.GetFileDictionaryPath().Value)
                    {
                        DbType = DbType.String
                    },
                    new SqlParameter("scriptName", query.FileName.Value)
                    {
                        DbType = DbType.String
                    }
                }
            };

        await command.ExecuteNonQueryAsync();
    }

    public async Task RunScriptAsync(Query query)
    {
        await using var command = new SqlCommand(query.QueryContent, _connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<DeployScript>> GetExecutedFile(DictionaryPath dictionaryPath)
    {
        var result = new List<DeployScript>();
        var query =
            $"SELECT name, path FROM {_deployScriptsTableName} WHERE path LIKE @scriptPath";

        await using var command = new SqlCommand(query, _connection)
        {
            Parameters = { new SqlParameter("scriptPath", dictionaryPath.Value) }
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