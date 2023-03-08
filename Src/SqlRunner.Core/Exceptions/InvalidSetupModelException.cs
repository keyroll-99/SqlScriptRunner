using SqlRunner.Core.Models;

namespace SqlRunner.Core.Exceptions;

public class InvalidSetupModelException : Exception
{
    private InvalidSetupModelException() : base("Invalid setup model")
    {
    }

    public static void ThrowIfInvalid(SetupModel model)
    {
        if (!model.IsValid)
        {
            throw new InvalidSetupModelException();
        }
    }
}