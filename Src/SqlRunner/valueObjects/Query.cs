using SqlRunner.exceptions;

namespace SqlRunner.valueObjects;

internal class Query
{
    public FileContent QueryContent { get; }
    public FileName FileName { get; }
    public FilePatch FilePatch { get; }

    public Query(FilePatch filePath)
    {
        FilePatch = filePath.GetPath();
        FileName = filePath.GetFileName();
        QueryContent = GetFileContent();
    }
    
    private FileContent GetFileContent()
    {
        var fileContent = File.ReadAllText(FilePatch);
        
        EmptyFileContentException.ThrowIfEmptyFileContent(fileContent, FilePatch);

        return fileContent;
    }

    public static implicit operator Query(string filePath) => new Query(filePath);
}