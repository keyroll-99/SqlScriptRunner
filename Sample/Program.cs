// See https://aka.ms/new-console-template for more information

using SqlRunner;
using SqlRunner.models;

var scriptRunner = SqlScriptRunner.GetScriptRunner(new SetupModel
{
    ConnectionString = "Server=localhost; Database=tests; User Id=sa; Password=pa55w0rd!;TrustServerCertificate=true",
    FolderPath = "/home/keyroll/dev/tests/mssql",
    DataBaseType = DataBaseTypeEnum.Mssql
});

scriptRunner.RunDeploy();