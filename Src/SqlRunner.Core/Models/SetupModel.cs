using SqlRunner.Core.ValueObject;

namespace SqlRunner.Core.Models;

public record SetupModel
{
    /// <summary>
    /// Connection string to database
    /// </summary>
    public string ConnectionString { get; init; }
    /// <summary>
    /// Path to root folder with sql script
    /// </summary>
    public DictionaryPath FolderPath { get; init; }
    /// <summary>
    /// Name of table where will be keep all deployed scripts
    /// </summary>
    public string DeployScriptsTableName { get; init; } = "DeployScripts";

    /// <summary>
    /// Path to folder with init scripts (like create database or create schema)
    /// </summary>
    public DictionaryPath? InitFolderPath { get; init; }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(ConnectionString)
        && !string.IsNullOrWhiteSpace(FolderPath)
        && !string.IsNullOrWhiteSpace(DeployScriptsTableName);
}
