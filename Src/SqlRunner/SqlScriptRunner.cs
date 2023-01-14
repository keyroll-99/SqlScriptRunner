using SqlRunner.models;
using SqlRunner.mssql;
using SqlRunner.mysql;
using SqlRunner.postgresql;

namespace SqlRunner;

public static class SqlScriptRunner
{
    public static IScriptRunner GetScriptRunner(SetupModel setupModel)
    {
        var scriptRunner = CreateScriptRunner(setupModel);
        try
        {
            scriptRunner.RunInitScriptsIfNotNull().Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return scriptRunner;
    }


    private static ScriptRunner CreateScriptRunner(SetupModel setupModel)
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