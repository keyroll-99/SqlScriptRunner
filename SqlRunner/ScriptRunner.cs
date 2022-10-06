using SqlRunner.models;

namespace SqlRunner;

public abstract class ScriptRunner : IDisposable
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
        var task = CloseConnectionAsync();
        task.Wait();
    }

    #endregion

    #region abstract

    protected abstract Task InitConnectionAsync();
    protected abstract Task CloseConnectionAsync();
    protected abstract Task<bool> IsDeployScriptTableExistsAsync();
    protected abstract Task CreateDeployScriptTable();
    protected abstract Task SaveLogAboutScriptRun(string filePath);
    protected abstract Task RunScriptAsync(string filePath);
    protected abstract Task<List<DeployScript>> GetExecutedFile(string path);

    #endregion

    protected ScriptRunner(SetupModel setupModel)
    {
        if (!setupModel.IsValid)
        {
            throw new ArgumentException(typeof(SetupModel).ToString());
        }

        SetupModel = setupModel;
    }
    
    internal async Task RunInitScripts()
    {
        if (SetupModel.InitFolderPath is null)
        {
            return;
        }
        await TryExecuteInitScript();
    }


    protected static string GetFileContent(string filePath)
    {
        var fileContent = File.ReadAllText(filePath);
        if (fileContent.Length == 0)
        {
            throw new ApplicationException("invalid file content lenght");
        }

        return fileContent;
    }

    protected static string GetPath(string pathToFile)
    {
        return pathToFile[..pathToFile.LastIndexOf(Path.DirectorySeparatorChar)];
    }

    protected static string GetFileName(string pathToFile)
    {
        return pathToFile.Split(Path.DirectorySeparatorChar).Last();
    }

    private async Task TryExecuteInitScript()
    {
        try
        {
            await InitConnectionAsync();
            if (await IsDeployScriptTableExistsAsync())
            {
                await ExecuteScripts(SetupModel.InitFolderPath, false);
            }
            else
            {
                var intiScripts = GetInitScript();
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

    private List<string> GetInitScript()
    {
        return Directory.EnumerateFiles(SetupModel.InitFolderPath!, "*.sql").ToList();
    }
    
    private async Task ExecuteScriptsWithoutSaveLog(List<string> sqlFiles)
    {
        foreach (var filePath in sqlFiles)
        {
            await RunScriptAsync(filePath);
        }
    }

    private async Task SaveLogsAboutInitScript(List<string> sqlFiles)
    {
        await CreateDeployScriptTable();
        foreach (var filePath in sqlFiles)
        {
            await SaveLogAboutScriptRun(filePath);
        }
    }

    private async Task ExecuteScripts()
    {
        await ExecuteScripts(SetupModel.FolderPath);
    }

    private async Task ExecuteScripts(string startPath, bool avoidInitFolder = true)
    {
        var directionToExecute = new Queue<string>();
        directionToExecute.Enqueue(startPath);

        while (directionToExecute.TryDequeue(out var directionPath))
        {
            if (avoidInitFolder && directionPath == SetupModel.InitFolderPath)
            {
                continue;
            }

            UpdateQueue(ref directionToExecute, directionPath);

            if (IsFolderDisabled(directionPath)) continue;

            foreach (var filePath in await GetSqlFiles(directionPath))
            {
                await TryExecuteScript(filePath);
            }
        }
    }

    private async Task TryExecuteScript(string filePath)
    {
        try
        {
            await RunScriptAsync(filePath);
            await SaveLogAboutScriptRun(filePath);
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

    private static void UpdateQueue(ref Queue<string> queue, string currentDirection)
    {
        foreach (var subDirectionPath in Directory.GetDirectories(currentDirection))
        {
            queue.Enqueue(subDirectionPath);
        }
    }

    private async Task<List<string>> GetSqlFiles(string path)
    {
        var allFiles = Directory.EnumerateFiles(path, "*.sql").ToList();
        var executedFiles = await GetExecutedFile(path);
        var result = allFiles
            .Where(x => executedFiles.All(y => $"{y.Path}/{y.Name}" != x))
            .ToList();

        return result;
    }
}