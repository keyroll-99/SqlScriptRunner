using SqlRunner.models;
using SqlRunner.valueObjects;

namespace SqlRunner.Abstraction;

internal interface IDatabaseScriptRunner
{ 
    Task InitConnectionAsync();
    Task CloseConnectionAsync();
    Task<bool> IsDeployScriptTableExistsAsync();
    Task CreateDeployScriptTable();
    Task SaveLogAboutScriptRun(Query query);
    Task RunScriptAsync(Query query);
    Task<List<DeployScript>> GetExecutedFile(FilePatch filePatch);
}