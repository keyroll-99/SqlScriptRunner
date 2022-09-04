using SqlRunner.models;
using SqlRunner.mssql;
using SqlRunner.mysql;
using SqlRunner.postgresql;

namespace SqlRunner;

public static class SqlScriptRunner
{
    public static ScriptRunner GetScriptRunner(SetupModel setupModel)
    {
        return setupModel.DataBaseType switch
        {
            DataBaseTypeEnum.Postgresql => new PostgresScriptRunner(setupModel),
            DataBaseTypeEnum.Mssql => new MssqlScriptRunner(setupModel),
            DataBaseTypeEnum.MySql => new MysqlScriptRunner(setupModel),
            _ => throw new ArgumentException($"Invalid argument {nameof(setupModel)}")
        };
    }
}