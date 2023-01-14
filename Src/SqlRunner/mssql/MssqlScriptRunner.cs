using System.Data;
using Microsoft.Data.SqlClient;
using SqlRunner.models;
using SqlRunner.valueObjects;

namespace SqlRunner.mssql;

internal class MssqlScriptRunner : ScriptRunner
{
    private readonly SqlConnection _connection;

    public MssqlScriptRunner(SetupModel setupModel) : base(setupModel)
    {
        _connection = new SqlConnection(setupModel.ConnectionString);
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
        const string sql = "Select 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @table;";

        await using var command = new SqlCommand(sql, _connection)
        {
            Parameters =
            {
                new SqlParameter("table", SetupModel.DeployScriptsTableName)
                {
                    DbType = DbType.String
                }
            }
        };
        await using var reader = await command.ExecuteReaderAsync();

        var result = await reader.ReadAsync();

        return result;
    }

    protected override async Task CreateDeployScriptTable()
    {
        var sql = $"CREATE TABLE {SetupModel.DeployScriptsTableName}(" +
                  "Id INT IDENTITY(1,1) PRIMARY KEY," +
                  "Path TEXT NOT NULL," +
                  "Name TEXT NOT NULL," +
                  "CreateDate DATETIME default getdate()" +
                  ")";

        await using var command = new SqlCommand(sql, _connection);
        await command.ExecuteNonQueryAsync();
    }

    protected override async Task SaveLogAboutScriptRun(Query query)
    {
        await using var command =
            new SqlCommand(
                $"INSERT INTO {SetupModel.DeployScriptsTableName} (Path, Name) VALUES (@scriptPath, @scriptName)",
                _connection
            )
            {
                Parameters =
                {
                    new SqlParameter("scriptPath", query.FilePatch)
                    {
                        DbType = DbType.String
                    },
                    new SqlParameter("scriptName", query.FileName)
                    {
                        DbType = DbType.String
                    }
                }
            };

        await command.ExecuteNonQueryAsync();
    }

    protected override async Task RunScriptAsync(Query query)
    {
        await using var command = new SqlCommand(query.QueryContent, _connection);
        await command.ExecuteNonQueryAsync();
    }

    protected override async Task<List<DeployScript>> GetExecutedFile(FilePatch path)
    {
        var result = new List<DeployScript>();
        var query =
            $"SELECT name, path FROM {SetupModel.DeployScriptsTableName} WHERE path LIKE @scriptPath";

        await using var command = new SqlCommand(query, _connection)
        {
            Parameters = { new SqlParameter("scriptPath", path) }
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