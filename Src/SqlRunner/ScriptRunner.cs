using SqlRunner.exceptions;
using SqlRunner.models;
using SqlRunner.valueObjects;

namespace SqlRunner;

public abstract class ScriptRunner : IDisposable, IAsyncDisposable, IScriptRunner
{
    protected readonly SetupModel SetupModel;

    #region public
    
    public void RunDeploy()
    {
        var task = RunDeployAsync();
        task.Wait();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task RunDeployAsync()
    {
        await InitConnectionAsync();

        try
        {
            if (!await IsDeployScriptTableExistsAsync())
            {
                await CreateDeployScriptTable();
            }

            await ExecuteScripts();
        }
        finally
        {
            await CloseConnectionAsync();
        }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    public async ValueTask DisposeAsync() => await CloseConnectionAsync();

    #endregion

    #region abstract

    protected abstract Task InitConnectionAsync();
    protected abstract Task CloseConnectionAsync();
    protected abstract Task<bool> IsDeployScriptTableExistsAsync();
    protected abstract Task CreateDeployScriptTable();
    protected abstract Task SaveLogAboutScriptRun(Query query);
    protected abstract Task RunScriptAsync(Query query);
    protected abstract Task<List<DeployScript>> GetExecutedFile(FilePatch filePatch);

    #endregion
    
    internal async Task RunInitScriptsIfNotNull()
    {
        if (SetupModel.InitFolderPath is null)
        {
            return;
        }
        await TryExecuteInitScript();
    }

    protected ScriptRunner(SetupModel setupModel)
    {
        InvalidSetupModelException.ThrowIfInvalid(setupModel);
        SetupModel = setupModel;
    }

    private async Task TryExecuteInitScript()
    {
        try
        {
            await InitConnectionAsync();
            if (await IsDeployScriptTableExistsAsync())
            {
                await ExecuteScripts(SetupModel.InitFolderPath!, false);
            }
            else
            {
                var intiScripts = GetInitScript().Select(x => new Query(x)).ToList();
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
            await CloseConnectionAsync();
        }
    }

    private IEnumerable<string> GetInitScript()
    {
        return Directory.EnumerateFiles(SetupModel.InitFolderPath!, "*.sql");
    }
    
    private async Task ExecuteScriptsWithoutSaveLog(IEnumerable<Query> queries)
    {
        foreach (var query in queries)
        {
            await RunScriptAsync(query);
        }
    }

    private async Task SaveLogsAboutInitScript(IEnumerable<Query> queries)
    {
        await CreateDeployScriptTable();
        foreach (var query in queries)
        {
            await SaveLogAboutScriptRun(query);
        }
    }

    private async Task ExecuteScripts()
    {
        await ExecuteScripts(SetupModel.FolderPath);
    }

    private async Task ExecuteScripts(FilePatch startPath, bool avoidInitFolder = true)
    {
        var directionToExecute = new Queue<FilePatch>();
        directionToExecute.Enqueue(startPath);

        while (directionToExecute.TryDequeue(out var directionPath))
        {
            if (avoidInitFolder && directionPath == SetupModel.InitFolderPath)
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
            await RunScriptAsync(query);
            await SaveLogAboutScriptRun(query);
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

    private static void UpdateQueue(ref Queue<FilePatch> queue, FilePatch currentDirection)
    {
        foreach (var subDirectionPath in Directory.GetDirectories(currentDirection))
        {
            queue.Enqueue(subDirectionPath);
        }
    }

    private async Task<IEnumerable<Query>> GetQueries(FilePatch path)
    {
        var allFiles = Directory.EnumerateFiles(path, "*.sql").ToList();
        var executedFiles = await GetExecutedFile(path);
        var result = allFiles
            .Where(x => executedFiles.All(y => $"{y.Path}/{y.Name}" != x))
            .Select(x => new Query(x));

        return result;
    }
}