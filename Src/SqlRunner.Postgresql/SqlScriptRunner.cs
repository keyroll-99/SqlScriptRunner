using SqlRunner.Abstraction;
using SqlRunner.Core.Models;

namespace SqlRunner.Postgresql;

public static class SqlScriptRunner
{
    public static IScriptRunner GetScriptRunner(SetupModel setupModel)
    {
        return new ScriptRunner(setupModel, new PostgresScriptRunner(setupModel.ConnectionString, setupModel.DeployScriptsTableName));
    }
}