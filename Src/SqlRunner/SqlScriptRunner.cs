using SqlRunner.Abstraction;
using SqlRunner.models;
using SqlRunner.mssql;
using SqlRunner.mysql;
using SqlRunner.postgresql;

namespace SqlRunner;

public static class SqlScriptRunner
{
    public static IScriptRunner GetScriptRunner(SetupModel setupModel)
        => new ScriptRunner(setupModel);
    
    
}