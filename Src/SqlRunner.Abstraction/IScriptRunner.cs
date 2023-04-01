namespace SqlRunner.Abstraction;

public interface IScriptRunner
{
    public void RunDeploy();
    public Task RunDeployAsync();
}