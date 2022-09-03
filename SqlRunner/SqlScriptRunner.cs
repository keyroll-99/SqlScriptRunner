using SqlRunner.models;
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
            DataBaseTypeEnum.MySql => throw new NotImplementedException(),
            _ => throw new ArgumentException($"Invalid argument {nameof(setupModel)}")
        };
    }
}