namespace SqlRunner.models;

public class SetupModel
{
    /// <summary>
    /// Connection string to database
    /// </summary>
    public string ConnectionString { get; init; }
    /// <summary>
    /// Path to root folder with sql script
    /// </summary>
    public string FolderPath { get; init; }
    /// <summary>
    /// Name of table where will be keep all deployed scripts
    /// </summary>
    public string DeployScriptsTableName { get; init; } = "DeployScripts";
    /// <summary>
    /// Typeof database
    /// </summary>
    public DataBaseTypeEnum DataBaseType { get; init; }
    
    /// <summary>
    /// Path to folder with init scripts (like create database or create schema)
    /// </summary>
    public string? InitFolderPath { get; init; }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(ConnectionString)
        && !string.IsNullOrWhiteSpace(FolderPath)
        && !string.IsNullOrWhiteSpace(DeployScriptsTableName)
        && Enum.IsDefined(typeof(DataBaseTypeEnum), DataBaseType);
}
