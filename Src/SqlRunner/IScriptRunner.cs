namespace SqlRunner;

public interface IScriptRunner
{
    public void RunDeploy();
    public Task RunDeployAsync();
}