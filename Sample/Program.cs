// See https://aka.ms/new-console-template for more information

using SqlRunner.Core.Models;
using SqlRunner.Postgresql;

var scriptRunner = SqlScriptRunner.GetScriptRunner(new SetupModel
{
    ConnectionString = "Server=127.0.0.1;Port=5432;Database=Tests;User Id=admin;Password=passwd;SearchPath=sql_script_runner;",
    FolderPath = "C:\\projects\\sqlScriptRunner\\SqlScriptRunner\\SampleScript",
    InitFolderPath = "C:\\projects\\sqlScriptRunner\\SqlScriptRunner\\SampleScript\\Init",
    DeployScriptsTableName = "UserDeployScript"
});

scriptRunner.RunDeploy();