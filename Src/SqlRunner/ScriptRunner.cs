using SqlRunner.Abstraction;
using SqlRunner.exceptions;
using SqlRunner.models;
using SqlRunner.valueObjects;

namespace SqlRunner;

internal class ScriptRunner : IDisposable, IAsyncDisposable, IScriptRunner
{
    private readonly SetupModel _setupModel;
    private readonly IDatabaseScriptRunner _databaseScriptRunner;

    public ScriptRunner(SetupModel setupModel, IDatabaseScriptRunner databaseScriptRunner)
    {
        InvalidSetupModelException.ThrowIfInvalid(setupModel);
        _setupModel = setupModel;
        _databaseScriptRunner = databaseScriptRunner;
    }

    public void RunDeploy()
    {
        var task = RunDeployAsync();
        task.Wait();
    }

    public async Task RunDeployAsync()
    {
        await RunInitScriptsIfNotNull();
        await _databaseScriptRunner.InitConnectionAsync();

        try
        {
            if (!await _databaseScriptRunner.IsDeployScriptTableExistsAsync())
            {
                await _databaseScriptRunner.CreateDeployScriptTable();
            }

            await ExecuteScripts();
        }
        finally
        {
            await _databaseScriptRunner.CloseConnectionAsync();
        }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    public async ValueTask DisposeAsync() => await _databaseScriptRunner.CreateDeployScriptTable();

    private async Task RunInitScriptsIfNotNull()
    {
        if (_setupModel.InitFolderPath is null)
        {
            return;
        }

        await TryExecuteInitScript();
    }


    private async Task TryExecuteInitScript()
    {
        try
        {
            await _databaseScriptRunner.InitConnectionAsync();
            if (await _databaseScriptRunner.IsDeployScriptTableExistsAsync())
            {
                await ExecuteScripts(_setupModel.InitFolderPath!, false);
            }
            else
            {
                var intiScripts = GetInitScript().Select(x => new Query(x));

                await ExecuteScriptsWithoutSaveLog(intiScripts);
                await SaveLogsAboutInitScript(intiScripts);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            await _databaseScriptRunner.CloseConnectionAsync();
        }
    }

    private IEnumerable<string> GetInitScript()
    {
        return Directory.EnumerateFiles(_setupModel.InitFolderPath!, "*.*", SearchOption.AllDirectories)
            .Where(x => x.EndsWith(".sql") || x.EndsWith(".Sql") || x.EndsWith(".SQL"));
    }

    private async Task ExecuteScriptsWithoutSaveLog(IEnumerable<Query> queries)
    {
        foreach (var query in queries)
        {
            await _databaseScriptRunner.RunScriptAsync(query);
        }
    }

    private async Task SaveLogsAboutInitScript(IEnumerable<Query> queries)
    {
        await _databaseScriptRunner.CreateDeployScriptTable();
        foreach (var query in queries)
        {
            await _databaseScriptRunner.SaveLogAboutScriptRun(query);
        }
    }

    private async Task ExecuteScripts()
    {
        await ExecuteScripts(_setupModel.FolderPath);
    }

    private async Task ExecuteScripts(DictionaryPath startPath, bool avoidInitFolder = true)
    {
        var directionToExecute = new Queue<DictionaryPath>();
        directionToExecute.Enqueue(startPath);

        while (directionToExecute.TryDequeue(out var directionPath))
        {
            if (avoidInitFolder && _setupModel.InitFolderPath?.Value is not null &&
                directionPath.Value.StartsWith(_setupModel.InitFolderPath))
            {
                continue;
            }

            UpdateQueue(ref directionToExecute, directionPath);

            if (IsFolderDisabled(directionPath)) continue;

            foreach (var query in await GetQueries(directionPath))
            {
                await TryExecuteScript(query);
            }
        }
    }

    private async Task TryExecuteScript(Query query)
    {
        try
        {
            await _databaseScriptRunner.RunScriptAsync(query);
            await _databaseScriptRunner.SaveLogAboutScriptRun(query);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static bool IsFolderDisabled(string path)
    {
        return File.Exists($"{path}/disable");
    }

    private static void UpdateQueue(ref Queue<DictionaryPath> queue, DictionaryPath currentDirection)
    {
        foreach (var subDirectionPath in Directory.GetDirectories(currentDirection))
        {
            queue.Enqueue(subDirectionPath);
        }
    }

    private async Task<IEnumerable<Query>> GetQueries(DictionaryPath path)
    {
        var allFiles = Directory.EnumerateFiles(path, "*.sql").ToList();
        var executedFiles = await _databaseScriptRunner.GetExecutedFile(path);
        var result = allFiles
            .Where(x => executedFiles.All(y => $"{y.Path}{Path.DirectorySeparatorChar}{y.Name}" != x))
            .Select(x => new Query(x));

        return result;
    }
}