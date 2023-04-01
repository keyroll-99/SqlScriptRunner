// See https://aka.ms/new-console-template for more information

using SqlRunner.Core.Models;
using SqlScriptRunnerPostgresql = SqlRunner.Postgresql.SqlScriptRunner;
using SqlScriptRunnerMssql = SqlRunner.Mssql.SqlScriptRunner;
using SqlScriptRunnerMySql = SqlRunner.MySql.SqlScriptRunner;


var scriptRunnerPostgresql = SqlScriptRunnerPostgresql.GetScriptRunner(new SetupModel
{
    ConnectionString = "Server=127.0.0.1;Port=5432;Database=Tests;User Id=admin;Password=passwd;SearchPath=sql_script_runner;",
    FolderPath = "C:\\projects\\sqlScriptRunner\\SqlScriptRunner\\SampleScriptPostgresql",
    InitFolderPath = "C:\\projects\\sqlScriptRunner\\SqlScriptRunner\\SampleScriptPostgresql\\Init",
    DeployScriptsTableName = "UserDeployScript"
});

var scriptRunnerMssql = SqlScriptRunnerMssql.GetScriptRunner(new SetupModel
{
    ConnectionString = "Server=localhost;Database=test;User Id=sa;Password=pa55w0rd!;Encrypt=False;",
    FolderPath = "C:\\projects\\sqlScriptRunner\\SqlScriptRunner\\SampleScriptMssql",
    DeployScriptsTableName = "UserDeployScript"
});

var scriptRunnerMySql = SqlScriptRunnerMySql.GetScriptRunner(new SetupModel
{
    ConnectionString = "server=localhost;uid=root;pwd=password123;database=test",
    FolderPath = "C:\\projects\\sqlScriptRunner\\SqlScriptRunner\\SampleScriptMySql",
    DeployScriptsTableName = "UserDeployScript"
});

var mssqlTask = scriptRunnerMssql.RunDeployAsync();
var postgresqlTask = scriptRunnerPostgresql.RunDeployAsync();
var scriptRunnerMySqlTask = scriptRunnerMySql.RunDeployAsync();

await Task.WhenAll(mssqlTask, postgresqlTask, scriptRunnerMySqlTask);

