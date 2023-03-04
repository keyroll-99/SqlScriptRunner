using FluentAssertions;
using Moq;
using SqlRunner;
using SqlRunner.Abstraction;
using SqlRunner.exceptions;
using SqlRunner.models;
using SqlRunner.valueObjects;
using Xunit;

namespace Test;

public class SqlScriptRunnerTest
{
    private readonly Mock<IDatabaseScriptRunner> _databaseScriptRunner = new();

    private readonly SetupModel _baseSetupModel;

    public SqlScriptRunnerTest()
    {
        _baseSetupModel = new SetupModel
        {
            ConnectionString = "ConnString",
            DataBaseType = DataBaseTypeEnum.Mssql,
            FolderPath = $".{Path.DirectorySeparatorChar}TestScripts",
            DeployScriptsTableName = "DeployTable",
            InitFolderPath = $".{Path.DirectorySeparatorChar}TestScripts{Path.DirectorySeparatorChar}Init"
        };

        _databaseScriptRunner.Setup(x => x.GetExecutedFile(It.IsAny<DictionaryPath>()))
            .ReturnsAsync(Enumerable.Empty<DeployScript>());
    }

    [Theory]
    [InlineData(null, "test", "name", 1)]
    [InlineData("connString", null, "name", 1)]
    [InlineData("connString", "test", "", 1)]
    [InlineData("connString", "test", "name", -1)]
    public void ScriptRunner_WhenInvalidSetupModel_ThenThrowException(string connectionString, string folderPath,
        string deployScriptsTableName, int dataBaseType)
    {
        // arrange
        var setupModel = new SetupModel()
        {
            ConnectionString = connectionString,
            FolderPath = folderPath,
            DataBaseType = (DataBaseTypeEnum)dataBaseType,
            DeployScriptsTableName = deployScriptsTableName
        };

        // act
        var scriptRunnerFunc = () => new ScriptRunner(setupModel, _databaseScriptRunner.Object);

        // arrange
        scriptRunnerFunc.Should().Throw<InvalidSetupModelException>().WithMessage("Invalid setup model");
    }

    [Fact]
    public async Task ScriptRunner_WhenGivenInitFolder_ThenFirstRunScriptFromInitFolder()
    {
        // arrange
        var setupModel = new SetupModel
        {
            ConnectionString = "ConnString",
            DataBaseType = DataBaseTypeEnum.Mssql,
            FolderPath = $".{Path.DirectorySeparatorChar}TestInitScripts",
            DeployScriptsTableName = "DeployTable",
            InitFolderPath = $".{Path.DirectorySeparatorChar}TestInitScripts{Path.DirectorySeparatorChar}Init"
        };

        // act
        var scriptRunner = new ScriptRunner(setupModel, _databaseScriptRunner.Object);
        await scriptRunner.RunDeployAsync();

        // arrange
        _databaseScriptRunner.Verify(x => x.InitConnectionAsync(), Times.Exactly(2));
        _databaseScriptRunner.Verify(x => x.CloseConnectionAsync(), Times.Exactly(2));
        _databaseScriptRunner.Verify(x => x.RunScriptAsync(It.Is<Query>(y => y.QueryContent == "InitScript")),
            Times.Once);

        _databaseScriptRunner.Verify(x => x.CreateDeployScriptTable());
        _databaseScriptRunner.Verify(x => x.SaveLogAboutScriptRun(It.Is<Query>(y => y.QueryContent == "SubInit")),
            Times.Once);
    }

    [Fact]
    public async Task ScriptRunner_WhenNotGivenInitFolder_ThenItDoesntRunScript()
    {
        // arrange
        var setupModel = new SetupModel
        {
            ConnectionString = "ConnString",
            DataBaseType = DataBaseTypeEnum.Mssql,
            FolderPath = "./TestInitScripts",
            DeployScriptsTableName = "DeployTable",
        };

        // act
        var scriptRunner = new ScriptRunner(setupModel, _databaseScriptRunner.Object);
        await scriptRunner.RunDeployAsync();

        // arrange
        _databaseScriptRunner.Verify(x => x.InitConnectionAsync(), Times.Exactly(1));
        _databaseScriptRunner.Verify(x => x.CloseConnectionAsync(), Times.Exactly(1));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ScriptRunner_WhenDeployScriptTableDoesntExists_ThenCreateIt(bool deployScriptExist)
    {
        // arrange
        _databaseScriptRunner.SetupSequence(x => x.IsDeployScriptTableExistsAsync()).ReturnsAsync(deployScriptExist)
            .ReturnsAsync(true);

        // act
        var scriptRunner = new ScriptRunner(_baseSetupModel, _databaseScriptRunner.Object);
        await scriptRunner.RunDeployAsync();

        // arrange
        _databaseScriptRunner.Verify(x => x.CreateDeployScriptTable(), Times.Exactly(deployScriptExist ? 0 : 1));
    }

    [Fact]
    public async Task ScriptRunner_WhenDeployRun_ShouldRunAllNotExecutedScript()
    {
        // arrange
        _databaseScriptRunner.Setup(x => x.GetExecutedFile(It.IsAny<DictionaryPath>()))
            .ReturnsAsync(new List<DeployScript>()
            {
                new()
                {
                    Name = "executed.sql",
                    Path = $".{Path.DirectorySeparatorChar}TestScripts{Path.DirectorySeparatorChar}executed"
                }
            });
        _databaseScriptRunner.Setup(x => x.IsDeployScriptTableExistsAsync()).ReturnsAsync(true);

        // act
        var scriptRunner = new ScriptRunner(_baseSetupModel, _databaseScriptRunner.Object);
        await scriptRunner.RunDeployAsync();

        // arrange

        #region openConnection

        _databaseScriptRunner.Verify(x => x.InitConnectionAsync(), Times.Exactly(2));
        _databaseScriptRunner.Verify(x => x.CloseConnectionAsync(), Times.Exactly(2));

        #endregion

        #region disabled

        _databaseScriptRunner.Verify(x => x.RunScriptAsync(It.Is<Query>(y => y.QueryContent == "it's disabled")),
            Times.Never);
        _databaseScriptRunner.Verify(x => x.SaveLogAboutScriptRun(It.Is<Query>(y => y.QueryContent == "it's disabled")),
            Times.Never);

        #endregion

        #region executed

        _databaseScriptRunner.Verify(x => x.RunScriptAsync(It.Is<Query>(y => y.QueryContent == "executed.sql")),
            Times.Never);
        _databaseScriptRunner.Verify(x => x.SaveLogAboutScriptRun(It.Is<Query>(y => y.QueryContent == "executed.sql")),
            Times.Never);

        _databaseScriptRunner.Verify(x => x.RunScriptAsync(It.Is<Query>(y => y.QueryContent == "not executed")),
            Times.Once);
        _databaseScriptRunner.Verify(x => x.SaveLogAboutScriptRun(It.Is<Query>(y => y.QueryContent == "not executed")),
            Times.Once);

        #endregion

        #region Init

        _databaseScriptRunner.Verify(x => x.RunScriptAsync(It.Is<Query>(y => y.QueryContent == "Inti sql")),
            Times.Once);
        _databaseScriptRunner.Verify(x => x.SaveLogAboutScriptRun(It.Is<Query>(y => y.QueryContent == "Inti sql")),
            Times.Once);

        #endregion

        #region part1

        _databaseScriptRunner.Verify(x => x.RunScriptAsync(It.Is<Query>(y => y.QueryContent == "user.sql")),
            Times.Once);
        _databaseScriptRunner.Verify(x => x.SaveLogAboutScriptRun(It.Is<Query>(y => y.QueryContent == "user.sql")),
            Times.Once);

        #endregion
    }
}