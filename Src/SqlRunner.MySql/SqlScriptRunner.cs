using SqlRunner.Abstraction;
using SqlRunner.Core.Models;

namespace SqlRunner.MySql;

public static class SqlScriptRunner
{
    public static IScriptRunner GetScriptRunner(SetupModel setupModel)
    {
        return new ScriptRunner(setupModel, new MysqlScriptRunner(setupModel.ConnectionString, setupModel.DeployScriptsTableName));
    }
}