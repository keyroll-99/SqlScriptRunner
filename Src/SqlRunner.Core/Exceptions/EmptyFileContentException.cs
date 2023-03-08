namespace SqlRunner.Core.Exceptions;

public class EmptyFileContentException : Exception
{
    private EmptyFileContentException(string filePath) : base($"File {filePath} is empty")
    {
    }

    public static void ThrowIfEmptyFileContent(string? fileContent, string file)
    {
        if (fileContent is null || fileContent.Length == 0)
        {
            throw new EmptyFileContentException(file);
        }
    }
}