namespace SqlRunner.Core.ValueObject;

public class FilePath
{
    public string Value { get; }

    public FilePath(string value)
    {
        Value = value;
    }
    
    internal DictionaryPath GetFileDictionaryPath()
    {
        return Value[..Value.LastIndexOf(Path.DirectorySeparatorChar)];
    }

    internal FileName GetFileName()
    {
        return Value.Split(Path.DirectorySeparatorChar).Last();
    }
    
    public static implicit operator string(FilePath filePath) => filePath.Value;
    public static implicit operator FilePath(string value) => new FilePath(value);
}