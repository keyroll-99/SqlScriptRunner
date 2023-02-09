using SqlRunner.Abstraction;
using SqlRunner.models;
using SqlRunner.mssql;
using SqlRunner.mysql;
using SqlRunner.postgresql;

namespace SqlRunner;

internal static class Database
{
    public static IDatabaseScriptRunner GetDatabaseScriptRunner(SetupModel setupModel)
    {
        return setupModel.DataBaseType switch
        {
            DataBaseTypeEnum.Postgresql => new PostgresScriptRunner(setupModel.ConnectionString, setupModel.DeployScriptsTableName),
            DataBaseTypeEnum.Mssql => new MssqlScriptRunner(setupModel.ConnectionString, setupModel.DeployScriptsTableName),
            DataBaseTypeEnum.MySql => new MysqlScriptRunner(setupModel.ConnectionString, setupModel.DeployScriptsTableName),
            _ => throw new ArgumentException($"Invalid argument {nameof(setupModel)}")
        };
    }
}