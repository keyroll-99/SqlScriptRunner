using SqlRunner.Core.Models;
using SqlRunner.Core.ValueObject;

namespace SqlRunner.Abstraction;

public interface IDatabaseScriptRunner
{ 
    Task InitConnectionAsync();
    Task CloseConnectionAsync();
    Task<bool> IsDeployScriptTableExistsAsync();
    Task CreateDeployScriptTable();
    Task SaveLogAboutScriptRun(Query query);
    Task RunScriptAsync(Query query);
    Task<IEnumerable<DeployScript>> GetExecutedFile(DictionaryPath filePatch);
}