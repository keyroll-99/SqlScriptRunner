// See https://aka.ms/new-console-template for more information

using SqlRunner;
using SqlRunner.models;

var scriptRunner = SqlScriptRunner.GetScriptRunner(new SetupModel
{
    ConnectionString = "erver=127.0.0.1;Port=5432;Database=test;User Id=admin;Password=passwd;",
    FolderPath = "/home/keyroll/dev/tests/sqlScript",
    DataBaseType = DataBaseTypeEnum.Postgresql
});

scriptRunner.RunDeploy();