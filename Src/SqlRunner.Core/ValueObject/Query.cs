using SqlRunner.Core.Exceptions;

namespace SqlRunner.Core.ValueObject;

public class Query
{
    public FileContent QueryContent { get; }
    public FileName FileName { get; }
    public FilePath FilePath { get; }

    public Query(FilePath filePath)
    {
        FilePath = filePath;
        FileName = filePath.GetFileName();
        QueryContent = GetFileContent();
    }
    
    private FileContent GetFileContent()
    {
        var fileContent = File.ReadAllText(FilePath);
        
        EmptyFileContentException.ThrowIfEmptyFileContent(fileContent, FilePath);

        return fileContent;
    }

    public static implicit operator Query(string filePath) => new Query(filePath);
}