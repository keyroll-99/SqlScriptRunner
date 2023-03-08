using SqlRunner.Abstraction;
using SqlRunner.Core.Models;

namespace SqlRunner.Mssql;

public static class SqlScriptRunner
{
    public static IScriptRunner GetScriptRunner(SetupModel setupModel)
    {
        return new ScriptRunner(setupModel, new MssqlScriptRunner(setupModel.ConnectionString, setupModel.DeployScriptsTableName));
    }
}