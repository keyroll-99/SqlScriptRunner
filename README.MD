# SQL SCRIPT RUNNER

SQL script runner is a package, wich run script from concrete folder and subfolder.
You can use it when you have database firs project and before start server you want to update your database.

## What's new in version 2.0!

From now the project will be separated into a few smaller packages. And now We don't have to install unnecessary connectors

-   SqlRunner: from now contains only a common runner for all databases
-   SqlRunner.Abstraction: contains only abstract level, you can use it to create your own runner
-   SqlRunner.Mssql, SqlRunner.MySql, SqlRunner.Postgresql - contains specifics runner for concrete database

## Setup model fields

-   ConnectionString: Connection string to database,
-   FolderPath: Path to root folder with sql script
-   DeployScriptsTableName: Name of table where will be keep all deployed scripts
-   DataBaseType: Typeof database (Postgresql, Mssql, Mysql)
-   InitFolderPath: Path to folder with init scripts (like create database or create schema)

## Example of use

```
var scriptRunner = SqlScriptRunner.GetScriptRunner(new SetupModel
{
    ConnectionString = "Server=localhost; Database=tests; User Id=sa; Password=pa55w0rd!;TrustServerCertificate=true",
    FolderPath = "/mssql/scripts",
    InitFolderPath = "/mssql/scripts/init",
    DataBaseType = DataBaseTypeEnum.Mssql
}).RunDeploy();
```

This example will be run all scripts (\*.sql file) from folder `/mssql/scripts` folder and subfolders
