// See https://aka.ms/new-console-template for more information

using SqlRunner;
using SqlRunner.models;

var scriptRunner = SqlScriptRunner.GetScriptRunner(new SetupModel
{
    ConnectionString = "Server=127.0.0.1;Port=5432;Database=crimeStory;User Id=admin;Password=passwd;SearchPath=user;",
    FolderPath = "/home/keyroll/dev/projects/crimeStory/api/CrimeStoryApi/src/User/User.Core/Scripts",
    InitFolderPath = "/home/keyroll/dev/projects/crimeStory/api/CrimeStoryApi/src/User/User.Core/Scripts/Init",
    DataBaseType = DataBaseTypeEnum.Postgresql,
    DeployScriptsTableName = "UserDeployScript"
});

scriptRunner.RunDeploy();