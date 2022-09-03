using SqlRunner.models;
using SqlRunner.mysql;
using SqlRunner.postgresql;

namespace SqlRunner;

public class SqlScriptRunner
{
    public static ScriptRunner? GetScriptRunner(SetupModel setupModel)
    {
        return setupModel.DataBaseType switch
        {
            DataBaseTypeEnum.Postgresql => new PostgresScriptRunner(setupModel),
            DataBaseTypeEnum.Mssql => throw new NotImplementedException(),
            DataBaseTypeEnum.MySql => new MysqlScriptRunner(setupModel),
            _ => throw new ArgumentException($"Invalid argument {nameof(setupModel)}")
        };
    }
}