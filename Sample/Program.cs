// See https://aka.ms/new-console-template for more information

using SqlRunner;
using SqlRunner.models;

var scriptRunner = SqlScriptRunner.GetScriptRunner(new SetupModel
{
    ConnectionString = "Server=127.0.0.1;Port=3307;Database=example;Uid=root;Password=password123;",
    FolderPath = "/home/keyroll/dev/tests/mysqlScript",
    DataBaseType = DataBaseTypeEnum.MySql
});

scriptRunner.RunDeploy();