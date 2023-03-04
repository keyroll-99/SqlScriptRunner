using System.Runtime.CompilerServices;
using SqlRunner.models;
using SqlRunner.valueObjects;

// this is necessary for mock it in unit tests
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace SqlRunner.Abstraction;

internal interface IDatabaseScriptRunner
{ 
    Task InitConnectionAsync();
    Task CloseConnectionAsync();
    Task<bool> IsDeployScriptTableExistsAsync();
    Task CreateDeployScriptTable();
    Task SaveLogAboutScriptRun(Query query);
    Task RunScriptAsync(Query query);
    Task<IEnumerable<DeployScript>> GetExecutedFile(DictionaryPath filePatch);
}