namespace SqlRunner.valueObjects;

public class FilePatch
{
    public string Value { get; }

    public FilePatch(string value)
    {
        Value = value;
    }
    
    internal FilePatch GetPath()
    {
        
        return Value[..Value.LastIndexOf(Path.DirectorySeparatorChar)];
    }

    internal FileName GetFileName()
    {
        return Value.Split(Path.DirectorySeparatorChar).Last();
    }
    
    public static implicit operator string(FilePatch filePatch) => filePatch.Value;
    public static implicit operator FilePatch(string value) => new FilePatch(value);
}